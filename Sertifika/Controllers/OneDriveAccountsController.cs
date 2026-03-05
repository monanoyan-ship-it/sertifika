using Sertifika.Entities;
using Sertifika.Factories.OneDriveAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/onedrive-accounts")]
[Authorize(Roles = "Admin")]
public class OneDriveAccountsController : ControllerBase
{
    private readonly IOneDriveAccountCrudFactory _crud;

    public OneDriveAccountsController(IOneDriveAccountCrudFactory crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OneDriveAccount>>> GetAccounts()
    {
        return Ok(await _crud.GetAccountsAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OneDriveAccount>> GetAccount(int id)
    {
        var account = await _crud.GetAccountAsync(id);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<OneDriveAccount>> CreateAccount([FromBody] OneDriveAccount account)
    {
        var created = await _crud.CreateAccountAsync(account);
        return CreatedAtAction(nameof(GetAccount), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] OneDriveAccount account)
    {
        if (id != account.Id) return BadRequest();
        await _crud.UpdateAccountAsync(account);
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _crud.SetDefaultAsync(id);
        return Ok(new { message = "Varsayilan hesap ayarlandi." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var found = await _crud.DeleteAccountAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }
}
