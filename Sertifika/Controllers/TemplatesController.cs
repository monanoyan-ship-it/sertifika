using System.Text.Json;
using System.Text.Json.Serialization;
using Sertifika.Entities;
using Sertifika.Factories.Templates;
using Sertifika.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateCrudFactory _crud;
    private readonly IWebHostEnvironment _env;
    private readonly IPdfService _pdfService;

    public TemplatesController(ITemplateCrudFactory crud, IWebHostEnvironment env, IPdfService pdfService)
    {
        _crud = crud;
        _env = env;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CertificateTemplate>>> GetTemplates()
    {
        return Ok(await _crud.GetTemplatesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CertificateTemplate>> GetTemplate(int id)
    {
        var template = await _crud.GetTemplateWithSignaturesAsync(id);
        if (template == null) return NotFound();
        return Ok(template);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<CertificateTemplate>> CreateTemplate(CertificateTemplate template)
    {
        var created = await _crud.CreateTemplateAsync(template);
        return CreatedAtAction(nameof(GetTemplate), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateTemplate(int id, CertificateTemplate template)
    {
        if (id != template.Id) return BadRequest();
        await _crud.UpdateTemplateAsync(template);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        var found = await _crud.DeleteTemplateAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }

    [HttpPut("{id}/signatures")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateSignatures(int id, [FromBody] UpdateTemplateSignaturesRequest request)
    {
        var template = await _crud.GetTemplateAsync(id);
        if (template == null) return NotFound();
        await _crud.UpdateTemplateSignaturesAsync(id, request.Signatures);
        return NoContent();
    }

    [HttpPost("{id}/preview")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> PreviewTemplate(int id, [FromBody] TemplatePreviewRequest request)
    {
        var template = await _crud.GetTemplateWithSignaturesAsync(id);
        if (template == null) return NotFound();

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

        var opts = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString };
        var fields = JsonSerializer.Deserialize<List<TemplateField>>(request.LayoutJson ?? "[]", opts) ?? new();

        var layout = fields.Select(f => new PdfFieldLayout
        {
            Type = f.FieldType == "dynamic" ? "dynamic" : (f.FieldType == "qrcode" ? "qrcode" : "static"),
            Key = f.DynamicKey,
            Text = f.StaticText,
            X = f.X, Y = f.Y, Width = f.Width, Height = f.Height,
            FontFamily = f.FontFamily,
            FontSize = f.FontSize,
            FontColor = f.FontColor,
            IsBold = f.IsBold, IsItalic = f.IsItalic,
            Alignment = f.TextAlign
        }).ToList();

        // Use editor's current signature positions (may be unsaved)
        var dbSignatures = (template.TemplateSignatures ?? new List<TemplateSignature>())
            .Where(ts => ts.IsActive)
            .OrderBy(ts => ts.DisplayOrder)
            .ToList();

        var signatures = new List<PdfSignatureInfo>();
        if (request.Signatures != null && request.Signatures.Count > 0)
        {
            foreach (var rs in request.Signatures)
            {
                var dbSig = dbSignatures.FirstOrDefault(ts => ts.SignatureId == rs.SignatureId);
                var imageUrl = dbSig != null ? ResolveFile(webRoot, dbSig.Signature.ImageUrl) : null;
                signatures.Add(new PdfSignatureInfo
                {
                    Name = rs.InstructorName ?? dbSig?.Signature.Name ?? "",
                    Title = rs.InstructorTitle ?? dbSig?.Signature.Title,
                    ImageUrl = imageUrl,
                    ShowName = rs.ShowName, ShowTitle = rs.ShowTitle,
                    ImageX = rs.ImageX, ImageY = rs.ImageY,
                    ImageWidth = rs.ImageWidth, ImageHeight = rs.ImageHeight,
                    ImageRotation = rs.ImageRotation,
                    NameX = rs.NameX, NameY = rs.NameY,
                    TitleX = rs.TitleX, TitleY = rs.TitleY,
                    NameFontSize = rs.NameFontSize, TitleFontSize = rs.TitleFontSize
                });
            }
        }
        else
        {
            signatures = dbSignatures.Select(ts => new PdfSignatureInfo
            {
                Name = ts.InstructorName ?? ts.Signature.Name,
                Title = ts.InstructorTitle ?? ts.Signature.Title,
                ImageUrl = ResolveFile(webRoot, ts.Signature.ImageUrl),
                ShowName = ts.ShowName, ShowTitle = ts.ShowTitle,
                ImageX = ts.ImageX, ImageY = ts.ImageY,
                ImageWidth = ts.ImageWidth, ImageHeight = ts.ImageHeight,
                ImageRotation = ts.ImageRotation,
                NameX = ts.NameX, NameY = ts.NameY,
                TitleX = ts.TitleX, TitleY = ts.TitleY,
                NameFontSize = ts.NameFontSize, TitleFontSize = ts.TitleFontSize
            }).ToList();
        }

        var dynamicValues = new Dictionary<string, string>
        {
            ["HolderName"] = "Ornek Katilimci",
            ["TrainingName"] = "Ornek Egitim Adi",
            ["TrainingDate"] = DateTime.Now.ToString("dd.MM.yyyy"),
            ["CompanyName"] = "Ornek Firma A.S.",
            ["CertificateNo"] = "CERT-ONIZLEME-0001",
            ["QrCode"] = "https://sertifika.example.com/verify/CERT-ONIZLEME-0001"
        };

        var orientation = (request.Orientation == 0) ? "landscape" : "portrait";
        string? bgPath = ResolveFile(webRoot, template.BackgroundImageUrl);

        var pdfBytes = await _pdfService.PreviewAsync(new PdfGenerateRequest
        {
            Orientation = orientation,
            BackgroundImagePath = bgPath,
            Layout = layout,
            Signatures = signatures,
            DynamicValues = dynamicValues
        });

        return File(pdfBytes, "application/pdf", "preview.pdf");
    }

    private static string? ResolveFile(string webRoot, string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        var fullPath = Path.Combine(webRoot, relativePath.TrimStart('/'));
        return System.IO.File.Exists(fullPath) ? fullPath : null;
    }

    [HttpPost("{id}/upload-background")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UploadBackground(int id, IFormFile file)
    {
        var template = await _crud.GetTemplateAsync(id);
        if (template == null) return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya gereklidir." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            return BadRequest(new { message = "Sadece PNG ve JPG dosyalari kabul edilir." });

        var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "backgrounds");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        template.BackgroundImageUrl = $"/uploads/backgrounds/{fileName}";
        await _crud.UpdateTemplateAsync(template);

        return Ok(new { backgroundImageUrl = template.BackgroundImageUrl });
    }
}

public class UpdateTemplateSignaturesRequest
{
    public List<TemplateSignatureInput> Signatures { get; set; } = new();
}

public class TemplatePreviewRequest
{
    public string? LayoutJson { get; set; }
    public int Orientation { get; set; }
    public List<TemplateSignatureInput>? Signatures { get; set; }
}
