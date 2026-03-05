using Sertifika.Entities;
using Sertifika.Factories.Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateCrudFactory _crud;

    public CertificatesController(ICertificateCrudFactory crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Certificate>>> GetCertificates()
    {
        return Ok(await _crud.GetCertificatesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Certificate>> GetCertificate(int id)
    {
        var certificate = await _crud.GetCertificateAsync(id);
        if (certificate == null) return NotFound();
        return certificate;
    }

    [HttpGet("holder/{holderId}")]
    public async Task<ActionResult<IEnumerable<Certificate>>> GetCertificatesByHolder(int holderId)
    {
        return Ok(await _crud.GetCertificatesByHolderAsync(holderId));
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Certificate>>> GetCertificatesByCategory(int categoryId)
    {
        return Ok(await _crud.GetCertificatesByCategoryAsync(categoryId));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<Certificate>> CreateCertificate(Certificate certificate)
    {
        var created = await _crud.CreateCertificateAsync(certificate);
        return CreatedAtAction(nameof(GetCertificate), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateCertificate(int id, Certificate certificate)
    {
        if (id != certificate.Id) return BadRequest();
        await _crud.UpdateCertificateAsync(certificate);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCertificate(int id)
    {
        var found = await _crud.DeleteCertificateAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }
}
