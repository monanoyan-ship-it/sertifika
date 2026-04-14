using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;
using Sertifika.Services;

namespace Sertifika.Factories.CertificateGeneration;

public class CertificateGenerationFactory : ICertificateGenerationFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ITrainingEntityService _trainingService;
    private readonly IParticipantEntityService _participantService;
    private readonly ITemplateEntityService _templateService;
    private readonly ICertificateSnapshotEntityService _snapshotService;
    private readonly IPdfService _pdfService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _env;

    public CertificateGenerationFactory(
        ITrainingEntityService trainingService,
        IParticipantEntityService participantService,
        ITemplateEntityService templateService,
        ICertificateSnapshotEntityService snapshotService,
        IPdfService pdfService,
        IUnitOfWork unitOfWork,
        IWebHostEnvironment env)
    {
        _trainingService = trainingService;
        _participantService = participantService;
        _templateService = templateService;
        _snapshotService = snapshotService;
        _pdfService = pdfService;
        _unitOfWork = unitOfWork;
        _env = env;
    }

    // ─── Generate: creates snapshots (no PDF on disk) ───

    public async Task<GenerationResult> GenerateCertificatesAsync(int trainingId)
    {
        var training = await _trainingService.GetByIdWithDetailsAsync(trainingId);
        if (training == null)
            throw new ArgumentException("Training not found");

        var participantList = (await _participantService.GetByTrainingIdAsync(trainingId)).ToList();
        if (participantList.Count == 0)
            throw new InvalidOperationException("Bu egitime henuz katilimci eklenmemis.");

        var template = training.Template;
        var layout = ParseLayout(template.LayoutJson);
        var signatures = await BuildSignatureListAsync(training);
        var orientation = template.Orientation == PageOrientation.Landscape ? "landscape" : "portrait";
        var backgroundPath = ResolveFilePath(template.BackgroundImageUrl);

        var result = new GenerationResult { Total = participantList.Count };

        foreach (var participant in participantList)
        {
            try
            {
                var certNumber = participant.CertificateNumber ?? GenerateCertificateNumber(training, participant);
                var dynamicValues = BuildDynamicValues(training, participant, certNumber);

                var existing = await _snapshotService.GetByParticipantIdAsync(participant.Id);
                if (existing == null)
                {
                    var snapshot = new CertificateSnapshot
                    {
                        ParticipantId = participant.Id,
                        CertificateNumber = certNumber,
                        Orientation = orientation,
                        BackgroundImagePath = backgroundPath,
                        LayoutJson = JsonSerializer.Serialize(layout, JsonOptions),
                        SignaturesJson = JsonSerializer.Serialize(signatures, JsonOptions),
                        DynamicValuesJson = JsonSerializer.Serialize(dynamicValues, JsonOptions),
                        GeneratedAt = DateTime.UtcNow
                    };
                    _snapshotService.Add(snapshot);
                }
                else
                {
                    existing.CertificateNumber = certNumber;
                    existing.Orientation = orientation;
                    existing.BackgroundImagePath = backgroundPath;
                    existing.LayoutJson = JsonSerializer.Serialize(layout, JsonOptions);
                    existing.SignaturesJson = JsonSerializer.Serialize(signatures, JsonOptions);
                    existing.DynamicValuesJson = JsonSerializer.Serialize(dynamicValues, JsonOptions);
                    existing.GeneratedAt = DateTime.UtcNow;
                }

                participant.CertificateNumber = certNumber;
                result.Success++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"{participant.FirstName} {participant.LastName}: {ex.Message}");
            }
        }

        training.Status = TrainingStatus.Generated;
        training.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return result;
    }

    // ─── Preview: always live render (no persistence) ───

    public async Task<byte[]> PreviewCertificateAsync(int trainingId, int? participantId = null)
    {
        var training = await _trainingService.GetByIdWithDetailsAsync(trainingId);
        if (training == null)
            throw new ArgumentException("Training not found");

        var template = training.Template;
        var layout = ParseLayout(template.LayoutJson);
        var signatures = await BuildSignatureListAsync(training);

        Dictionary<string, string> dynamicValues;
        if (participantId.HasValue)
        {
            var participant = await _participantService.GetByIdAsync(participantId.Value);
            if (participant == null)
                throw new ArgumentException("Participant not found");
            var certNumber = participant.CertificateNumber ?? GenerateCertificateNumber(training, participant);
            dynamicValues = BuildDynamicValues(training, participant, certNumber);
        }
        else
        {
            dynamicValues = new Dictionary<string, string>
            {
                ["HolderName"] = "Ornek Ad Soyad",
                ["TrainingName"] = training.Name,
                ["ProgramName"] = training.DisplayName ?? training.Name,
                ["TrainingDate"] = training.FormatDateRange(),
                ["CompanyName"] = training.CompanyName ?? "",
                ["CertificateNo"] = "CERT-ONIZLEME-001",
                ["InstructorName"] = training.InstructorName ?? "Ornek Egitmen",
                ["QrCode"] = "https://sertifika.example.com/verify/CERT-ONIZLEME-001"
            };
        }

        return await _pdfService.PreviewAsync(new PdfGenerateRequest
        {
            Orientation = template.Orientation == PageOrientation.Landscape ? "landscape" : "portrait",
            BackgroundImagePath = ResolveFilePath(template.BackgroundImageUrl),
            Layout = layout,
            Signatures = signatures,
            DynamicValues = dynamicValues
        });
    }

    // ─── Render from snapshot (on-demand PDF) ───

    public async Task<byte[]?> RenderFromSnapshotAsync(string certificateNumber)
    {
        var snapshot = await _snapshotService.GetByCertificateNumberAsync(certificateNumber);
        return snapshot == null ? null : await RenderAsync(snapshot);
    }

    public async Task<byte[]?> RenderByParticipantAsync(int participantId)
    {
        var snapshot = await _snapshotService.GetByParticipantIdAsync(participantId);
        return snapshot == null ? null : await RenderAsync(snapshot);
    }

    private async Task<byte[]> RenderAsync(CertificateSnapshot snapshot)
    {
        var layout = JsonSerializer.Deserialize<List<PdfFieldLayout>>(snapshot.LayoutJson, JsonOptions) ?? new();
        var signatures = JsonSerializer.Deserialize<List<PdfSignatureInfo>>(snapshot.SignaturesJson, JsonOptions) ?? new();
        var dynamicValues = JsonSerializer.Deserialize<Dictionary<string, string>>(snapshot.DynamicValuesJson, JsonOptions) ?? new();

        var participant = snapshot.Participant;
        var training = participant.Training;
        var filename = BuildFilename(training, participant);

        return await _pdfService.GenerateSingleAsync(new PdfGenerateRequest
        {
            Orientation = snapshot.Orientation,
            BackgroundImagePath = snapshot.BackgroundImagePath,
            Layout = layout,
            Signatures = signatures,
            DynamicValues = dynamicValues,
            OutputFilename = filename
        });
    }

    // ─── ZIP: render all snapshots on-demand ───

    public async Task<byte[]> DownloadZipAsync(int trainingId)
    {
        var training = await _trainingService.GetByIdWithDetailsAsync(trainingId);
        if (training == null)
            throw new ArgumentException("Training not found");

        var snapshots = (await _snapshotService.GetByTrainingIdAsync(trainingId)).ToList();
        if (snapshots.Count == 0)
            throw new InvalidOperationException("Henuz sertifika uretilmemis. Once 'Sertifika Uret' butonuna basin.");

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var snapshot in snapshots)
            {
                var pdfBytes = await RenderAsync(snapshot);
                var fileName = BuildFilename(training, snapshot.Participant);
                var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(pdfBytes);
            }
        }

        return memoryStream.ToArray();
    }

    // ─── Helpers ───

    private List<PdfFieldLayout> ParseLayout(string layoutJson)
    {
        var fields = JsonSerializer.Deserialize<List<TemplateField>>(layoutJson, JsonOptions) ?? new();
        return fields
            .OrderBy(f => f.DisplayOrder)
            .Select(f => new PdfFieldLayout
            {
                Type = f.FieldType == "dynamic" ? "dynamic" : (f.DynamicKey == "QrCode" ? "qrcode" : (f.FieldType == "qrcode" ? "qrcode" : "static")),
                Key = f.DynamicKey,
                Text = f.StaticText,
                X = f.X,
                Y = f.Y,
                Width = f.Width,
                Height = f.Height,
                FontFamily = f.FontFamily,
                FontSize = f.FontSize,
                FontColor = f.FontColor,
                IsBold = f.IsBold,
                IsItalic = f.IsItalic,
                IsUnderline = f.IsUnderline,
                LetterSpacing = f.LetterSpacing,
                LineHeight = f.LineHeight,
                Alignment = f.TextAlign
            }).ToList();
    }

    private async Task<List<PdfSignatureInfo>> BuildSignatureListAsync(Training training)
    {
        var trainingSignatures = training.TrainingSignatures
            .Where(ts => ts.IsActive)
            .OrderBy(ts => ts.DisplayOrder)
            .ToList();

        if (trainingSignatures.Count > 0 && trainingSignatures.Any(ts => ts.ImageX != 0 || ts.ImageY != 0))
        {
            return trainingSignatures.Select(ts => new PdfSignatureInfo
            {
                Name = ts.InstructorName ?? ts.Signature.Name,
                Title = ts.InstructorTitle ?? ts.Signature.Title,
                ImageUrl = ResolveFilePath(ts.Signature.ImageUrl),
                ShowName = ts.ShowName,
                ShowTitle = ts.ShowTitle,
                ImageX = ts.ImageX,
                ImageY = ts.ImageY,
                ImageWidth = ts.ImageWidth,
                ImageHeight = ts.ImageHeight,
                ImageRotation = ts.ImageRotation,
                NameX = ts.NameX,
                NameY = ts.NameY,
                TitleX = ts.TitleX,
                TitleY = ts.TitleY,
                NameFontSize = ts.NameFontSize,
                TitleFontSize = ts.TitleFontSize
            }).ToList();
        }

        var template = await _templateService.GetByIdWithSignaturesAsync(training.TemplateId);
        if (template?.TemplateSignatures == null) return new();

        return template.TemplateSignatures
            .Where(ts => ts.IsActive)
            .OrderBy(ts => ts.DisplayOrder)
            .Select(ts => new PdfSignatureInfo
            {
                Name = ts.InstructorName ?? ts.Signature.Name,
                Title = ts.InstructorTitle ?? ts.Signature.Title,
                ImageUrl = ResolveFilePath(ts.Signature.ImageUrl),
                ShowName = ts.ShowName,
                ShowTitle = ts.ShowTitle,
                ImageX = ts.ImageX,
                ImageY = ts.ImageY,
                ImageWidth = ts.ImageWidth,
                ImageHeight = ts.ImageHeight,
                ImageRotation = ts.ImageRotation,
                NameX = ts.NameX,
                NameY = ts.NameY,
                TitleX = ts.TitleX,
                TitleY = ts.TitleY,
                NameFontSize = ts.NameFontSize,
                TitleFontSize = ts.TitleFontSize
            }).ToList();
    }

    private Dictionary<string, string> BuildDynamicValues(Training training, Participant participant, string certNumber)
    {
        return new Dictionary<string, string>
        {
            ["HolderName"] = $"{participant.FirstName} {participant.LastName}",
            ["TrainingName"] = training.Name,
            ["ProgramName"] = training.DisplayName ?? training.Name,
            ["TrainingDate"] = training.FormatDateRange(),
            ["CompanyName"] = participant.CompanyName ?? training.CompanyName ?? "",
            ["CertificateNo"] = certNumber,
            ["InstructorName"] = training.InstructorName ?? "",
            ["QrCode"] = $"/api/certificates/verify/{certNumber}"
        };
    }

    private string GenerateCertificateNumber(Training training, Participant participant)
        => $"CERT-{DateTime.UtcNow:yyyyMMdd}-{training.Id:D4}-{participant.Id:D4}";

    private string BuildFilename(Training training, Participant participant)
    {
        var safe = (string s) => string.Concat(s.Select(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_'));
        var company = safe(participant.CompanyName ?? training.CompanyName ?? "Genel");
        var trainingName = safe(training.Name);
        var date = training.TrainingDate.ToString("yyyyMMdd");
        var name = safe($"{participant.FirstName}_{participant.LastName}");
        return $"{company}_{trainingName}_{date}_{name}.pdf";
    }

    private string? ResolveFilePath(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(webRoot, relativePath.TrimStart('/'));
        return File.Exists(fullPath) ? fullPath : null;
    }
}
