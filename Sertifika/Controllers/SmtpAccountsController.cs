using Sertifika.Entities;
using Sertifika.Factories.SmtpAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/smtp-accounts")]
[Authorize]
public class SmtpAccountsController : ControllerBase
{
    private readonly ISmtpAccountCrudFactory _crud;

    public SmtpAccountsController(ISmtpAccountCrudFactory crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SmtpAccount>>> GetAccounts()
    {
        return Ok(await _crud.GetAccountsAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SmtpAccount>> GetAccount(int id)
    {
        var account = await _crud.GetAccountAsync(id);
        if (account == null) return NotFound();
        return account;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SmtpAccount>> CreateAccount(SmtpAccount account)
    {
        var created = await _crud.CreateAccountAsync(account);
        return CreatedAtAction(nameof(GetAccount), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAccount(int id, SmtpAccount account)
    {
        if (id != account.Id) return BadRequest();
        await _crud.UpdateAccountAsync(account);
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _crud.SetDefaultAsync(id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var found = await _crud.DeleteAccountAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }
}
