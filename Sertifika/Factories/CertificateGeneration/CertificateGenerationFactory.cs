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
    private readonly ITrainingEntityService _trainingService;
    private readonly IParticipantEntityService _participantService;
    private readonly ITemplateEntityService _templateService;
    private readonly IPdfService _pdfService;
    private readonly IOneDriveService _oneDrive;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _env;

    public CertificateGenerationFactory(
        ITrainingEntityService trainingService,
        IParticipantEntityService participantService,
        ITemplateEntityService templateService,
        IPdfService pdfService,
        IOneDriveService oneDrive,
        IUnitOfWork unitOfWork,
        IWebHostEnvironment env)
    {
        _trainingService = trainingService;
        _participantService = participantService;
        _templateService = templateService;
        _pdfService = pdfService;
        _oneDrive = oneDrive;
        _unitOfWork = unitOfWork;
        _env = env;
    }

    public async Task<GenerationResult> GenerateCertificatesAsync(int trainingId)
    {
        var training = await _trainingService.GetByIdWithDetailsAsync(trainingId);
        if (training == null)
            throw new ArgumentException("Training not found");

        var participants = await _participantService.GetByTrainingIdAsync(trainingId);
        var participantList = participants.ToList();

        if (participantList.Count == 0)
            throw new InvalidOperationException("No participants found for this training");

        var template = training.Template;
        var layout = ParseLayout(template.LayoutJson);
        var signatures = await BuildSignatureListAsync(training);

        var outputDir = Path.Combine(
            _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
            "uploads", "certificates", $"training_{trainingId}");
        Directory.CreateDirectory(outputDir);

        var result = new GenerationResult { Total = participantList.Count };

        foreach (var participant in participantList)
        {
            try
            {
                var certNumber = participant.CertificateNumber ?? GenerateCertificateNumber(training, participant);
                var dynamicValues = BuildDynamicValues(training, participant, certNumber);
                var filename = BuildFilename(training, participant);

                var pdfBytes = await _pdfService.GenerateSingleAsync(new PdfGenerateRequest
                {
                    Orientation = template.Orientation == PageOrientation.Landscape ? "landscape" : "portrait",
                    BackgroundImagePath = ResolveFilePath(template.BackgroundImageUrl),
                    Layout = layout,
                    Signatures = signatures,
                    DynamicValues = dynamicValues,
                    OutputFilename = filename
                });

                var filePath = Path.Combine(outputDir, filename);
                await File.WriteAllBytesAsync(filePath, pdfBytes);

                participant.CertificateNumber = certNumber;
                participant.CertificatePdfUrl = $"/uploads/certificates/training_{trainingId}/{filename}";
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
                ["TrainingName"] = training.DisplayName ?? training.Name,
                ["TrainingDate"] = training.TrainingDate.ToString("dd.MM.yyyy"),
                ["CompanyName"] = training.CompanyName ?? "",
                ["CertificateNo"] = "CERT-ONIZLEME-001",
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

    public async Task<byte[]> DownloadZipAsync(int trainingId)
    {
        var training = await _trainingService.GetByIdWithDetailsAsync(trainingId);
        if (training == null)
            throw new ArgumentException("Training not found");

        var participants = (await _participantService.GetByTrainingIdAsync(trainingId))
            .Where(p => !string.IsNullOrEmpty(p.CertificatePdfUrl))
            .ToList();

        if (participants.Count == 0)
            throw new InvalidOperationException("No certificates generated yet. Run generate first.");

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var participant in participants)
            {
                var fileName = Path.GetFileName(participant.CertificatePdfUrl!);
                byte[]? pdfBytes = null;

                if (participant.StorageType == CertificateStorageType.OneDrive && !string.IsNullOrEmpty(participant.CloudFileId))
                {
                    pdfBytes = await _oneDrive.DownloadFileAsync(participant.CloudFileId);
                }
                else
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                    var localPath = Path.Combine(webRoot, participant.CertificatePdfUrl!.TrimStart('/'));
                    if (File.Exists(localPath))
                        pdfBytes = await File.ReadAllBytesAsync(localPath);
                }

                if (pdfBytes != null)
                {
                    var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(pdfBytes);
                }
            }
        }

        return memoryStream.ToArray();
    }

    private List<PdfFieldLayout> ParseLayout(string layoutJson)
    {
        var opts = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString };
        var fields = JsonSerializer.Deserialize<List<TemplateField>>(layoutJson, opts) ?? new();
        return fields.Select(f => new PdfFieldLayout
        {
            Type = f.FieldType == "dynamic" ? "dynamic" : (f.DynamicKey == "QrCode" ? "qrcode" : "static"),
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
            Alignment = f.TextAlign
        }).ToList();
    }

    private async Task<List<PdfSignatureInfo>> BuildSignatureListAsync(Training training)
    {
        // If training has its own signatures with positions, use them
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

        // Fallback: use template signatures (training may have been created before signatures were configured)
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
            ["TrainingName"] = training.DisplayName ?? training.Name,
            ["TrainingDate"] = training.TrainingDate.ToString("dd.MM.yyyy"),
            ["CompanyName"] = participant.CompanyName ?? training.CompanyName ?? "",
            ["CertificateNo"] = certNumber,
            ["QrCode"] = $"/api/certificates/verify/{certNumber}"
        };
    }

    private string GenerateCertificateNumber(Training training, Participant participant)
    {
        var date = DateTime.UtcNow;
        return $"CERT-{date:yyyyMMdd}-{training.Id:D4}-{participant.Id:D4}";
    }

    private string BuildFilename(Training training, Participant participant)
    {
        var company = (participant.CompanyName ?? training.CompanyName ?? "Genel").Replace(" ", "_");
        var trainingName = training.Name.Replace(" ", "_");
        var date = training.TrainingDate.ToString("yyyyMMdd");
        var name = $"{participant.FirstName}_{participant.LastName}";
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
