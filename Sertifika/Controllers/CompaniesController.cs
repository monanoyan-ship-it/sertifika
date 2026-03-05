using Sertifika.Entities;
using Sertifika.Factories.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyCrudFactory _crud;

    public CompaniesController(ICompanyCrudFactory crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
        return Ok(await _crud.GetCompaniesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> GetCompany(int id)
    {
        var company = await _crud.GetCompanyAsync(id);
        if (company == null) return NotFound();
        return company;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<Company>> CreateCompany(Company company)
    {
        var created = await _crud.CreateCompanyAsync(company);
        return CreatedAtAction(nameof(GetCompany), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateCompany(int id, Company company)
    {
        if (id != company.Id) return BadRequest();
        await _crud.UpdateCompanyAsync(company);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var found = await _crud.DeleteCompanyAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }
}
