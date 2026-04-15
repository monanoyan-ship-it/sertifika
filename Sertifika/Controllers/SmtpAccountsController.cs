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
    public async Task<ActionResult<IEnumerable<object>>> GetAccounts()
    {
        var accounts = await _crud.GetAccountsAsync();
        return Ok(accounts.Select(ToMasked));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetAccount(int id)
    {
        var account = await _crud.GetAccountAsync(id);
        if (account == null) return NotFound();
        return Ok(ToMasked(account));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> CreateAccount([FromBody] SmtpAccountUpsertRequest req)
    {
        var created = await _crud.CreateAccountAsync(FromRequest(req, 0));
        return CreatedAtAction(nameof(GetAccount), new { id = created.Id }, ToMasked(created));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] SmtpAccountUpsertRequest req)
    {
        await _crud.UpdateAccountAsync(
            FromRequest(req, id),
            updatePassword: !string.IsNullOrEmpty(req.Password));
        return NoContent();
    }

    private static SmtpAccount FromRequest(SmtpAccountUpsertRequest req, int id) => new()
    {
        Id = id,
        Name = req.Name,
        Host = req.Host,
        Port = req.Port,
        Username = req.Username,
        Password = req.Password ?? "",
        FromEmail = req.FromEmail,
        FromName = req.FromName,
        UseSsl = req.UseSsl,
        UseOAuth = req.UseOAuth,
        UseGraphApi = req.UseGraphApi,
        TenantId = req.TenantId,
        ClientId = req.ClientId,
        ClientSecret = req.ClientSecret ?? "",
        IsDefault = req.IsDefault,
        IsEnabled = req.IsEnabled
    };

    [HttpPost("{id}/set-default")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _crud.SetDefaultAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/toggle-enabled")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleEnabled(int id, [FromBody] ToggleEnabledRequest req)
    {
        await _crud.SetEnabledAsync(id, req.Enabled);
        return NoContent();
    }

    [HttpPost("{id}/test-connection")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> TestConnection(int id)
    {
        var result = await _crud.TestConnectionAsync(id);
        return Ok(new { success = result.Success, error = result.Error });
    }

    [HttpPost("{id}/send-test")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> SendTest(int id, [FromBody] SendTestRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ToEmail))
            return BadRequest(new { error = "Test maili gondermek icin bir e-posta adresi girin." });

        var result = await _crud.SendTestEmailAsync(id, req.ToEmail.Trim());
        return Ok(new { success = result.Success, error = result.Error });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var found = await _crud.DeleteAccountAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }

    private static object ToMasked(SmtpAccount a) => new
    {
        a.Id,
        a.Name,
        a.Host,
        a.Port,
        a.Username,
        a.FromEmail,
        a.FromName,
        a.UseSsl,
        a.UseOAuth,
        a.UseGraphApi,
        a.TenantId,
        a.ClientId,
        a.IsDefault,
        a.IsEnabled,
        a.LastTestedAt,
        a.LastTestStatus,
        a.LastTestError,
        a.CreatedAt,
        HasPassword = !string.IsNullOrEmpty(a.Password),
        HasClientSecret = !string.IsNullOrEmpty(a.ClientSecret),
        AuthMode = a.UseGraphApi ? "graph" : (a.UseOAuth ? "oauth" : "basic")
    };
}

public class SmtpAccountUpsertRequest
{
    public string Name { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string? Password { get; set; }
    public string FromEmail { get; set; } = "";
    public string? FromName { get; set; }
    public bool UseSsl { get; set; } = true;
    public bool UseOAuth { get; set; }
    public bool UseGraphApi { get; set; }
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class ToggleEnabledRequest
{
    public bool Enabled { get; set; }
}

public class SendTestRequest
{
    public string ToEmail { get; set; } = "";
}
