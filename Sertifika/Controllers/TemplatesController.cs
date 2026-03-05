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

    public TemplatesController(ITemplateCrudFactory crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CertificateTemplate>>> GetTemplates()
    {
        return Ok(await _crud.GetTemplatesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CertificateTemplate>> GetTemplate(int id)
    {
        var template = await _crud.GetTemplateAsync(id);
        if (template == null) return NotFound();
        return template;
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
}
