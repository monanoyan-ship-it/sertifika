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
        => Ok(await _crud.GetCompaniesAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> GetCompany(int id)
    {
        var company = await _crud.GetCompanyAsync(id);
        if (company == null) return NotFound();
        return company;
    }

    [HttpGet("{id}/detail")]
    public async Task<ActionResult<CompanyDetailDto>> GetCompanyDetail(int id)
    {
        var detail = await _crud.GetCompanyDetailAsync(id);
        if (detail == null) return NotFound();
        return Ok(detail);
    }

    [HttpGet("{id}/contacts")]
    public async Task<ActionResult<IEnumerable<Contact>>> GetContacts(int id)
        => Ok(await _crud.GetContactsAsync(id));

    [HttpPost("{id}/contacts")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<Contact>> CreateContact(int id, [FromBody] ContactUpsertRequest request)
    {
        var contact = new Contact
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone
        };
        var created = await _crud.CreateContactAsync(id, contact);
        return Ok(created);
    }

    [HttpPut("{id}/contacts/{contactId}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateContact(int id, int contactId, [FromBody] ContactUpsertRequest request)
    {
        await _crud.UpdateContactAsync(id, new Contact
        {
            Id = contactId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone
        });
        return NoContent();
    }

    [HttpDelete("{id}/contacts/{contactId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteContact(int id, int contactId)
    {
        var found = await _crud.DeleteContactAsync(id, contactId);
        if (!found) return NotFound();
        return NoContent();
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

public class ContactUpsertRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
