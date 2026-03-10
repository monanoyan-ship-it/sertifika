using Sertifika.Entities;
using Sertifika.Factories.EmailTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/email-templates")]
[Authorize]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateCrudFactory _crud;

    public EmailTemplatesController(IEmailTemplateCrudFactory crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailTemplate>>> GetTemplates()
    {
        return Ok(await _crud.GetTemplatesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmailTemplate>> GetTemplate(int id)
    {
        var template = await _crud.GetTemplateAsync(id);
        if (template == null) return NotFound();
        return template;
    }

    [HttpGet("placeholders")]
    [AllowAnonymous]
    public ActionResult GetPlaceholders()
    {
        return Ok(new[]
        {
            new { code = "{RecipientName}", label = "Alici Ad Soyad" },
            new { code = "{TrainingName}", label = "Egitim Adi" },
            new { code = "{TrainingDate}", label = "Egitim Tarihi" },
            new { code = "{CompanyName}", label = "Firma Adi" },
            new { code = "{CertificateNo}", label = "Sertifika No" }
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<EmailTemplate>> CreateTemplate(EmailTemplate template)
    {
        var created = await _crud.CreateTemplateAsync(template);
        return CreatedAtAction(nameof(GetTemplate), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateTemplate(int id, EmailTemplate template)
    {
        if (id != template.Id) return BadRequest();
        await _crud.UpdateTemplateAsync(template);
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _crud.SetDefaultAsync(id);
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
}
