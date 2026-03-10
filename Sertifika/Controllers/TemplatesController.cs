using Sertifika.Entities;
using Sertifika.Factories.Templates;
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

    public TemplatesController(ITemplateCrudFactory crud, IWebHostEnvironment env)
    {
        _crud = crud;
        _env = env;
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
