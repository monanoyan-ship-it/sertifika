using Sertifika.Entities;
using Sertifika.Factories.OneDriveAccounts;
using Sertifika.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/onedrive-accounts")]
[Authorize(Roles = "Admin")]
public class OneDriveAccountsController : ControllerBase
{
    private readonly IOneDriveAccountCrudFactory _crud;
    private readonly OneDriveOAuthService _oAuth;
    private readonly IOneDriveService _oneDriveService;
    private readonly string _webAppBaseUrl;

    public OneDriveAccountsController(
        IOneDriveAccountCrudFactory crud,
        OneDriveOAuthService oAuth,
        IOneDriveService oneDriveService,
        IConfiguration config)
    {
        _crud = crud;
        _oAuth = oAuth;
        _oneDriveService = oneDriveService;
        _webAppBaseUrl = config["WebApp:BaseUrl"] ?? "";
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAccounts()
    {
        var accounts = await _crud.GetAccountsAsync();
        var masked = accounts.Select(a => new
        {
            a.Id,
            a.Name,
            a.TenantId,
            a.DriveId,
            a.IsDefault,
            a.IsActive,
            a.CreatedAt,
            HasRefreshToken = !string.IsNullOrEmpty(a.RefreshToken),
            HasClientSecret = !string.IsNullOrEmpty(a.ClientSecret)
        });
        return Ok(masked);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetAccount(int id)
    {
        var a = await _crud.GetAccountAsync(id);
        if (a == null) return NotFound();
        return Ok(new
        {
            a.Id,
            a.Name,
            a.TenantId,
            a.ClientId,
            a.DriveId,
            a.DriveUserId,
            a.IsDefault,
            a.IsActive,
            a.CreatedAt,
            HasRefreshToken = !string.IsNullOrEmpty(a.RefreshToken),
            HasClientSecret = !string.IsNullOrEmpty(a.ClientSecret)
        });
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

    // ─── OneDrive OAuth2 Flow ───

    [HttpGet("oauth/auth-url")]
    public ActionResult GetAuthUrl([FromQuery] string? tenantId)
    {
        if (!_oAuth.IsConfigured)
            return BadRequest(new { error = "OneDrive OAuth yapilandirilmamis. appsettings'te OneDrive:ClientId/ClientSecret ayarlayin." });

        var state = Guid.NewGuid().ToString("N");
        var url = _oAuth.GetAuthorizationUrl(tenantId, state);
        return Ok(new { authUrl = url, state });
    }

    [HttpGet("oauth/callback")]
    [AllowAnonymous]
    public IActionResult OAuthCallback([FromQuery] string? code, [FromQuery] string? error, [FromQuery] string? error_description)
    {
        if (!string.IsNullOrEmpty(code))
            return Redirect($"/Panel/OAuthCallback?provider=onedrive&code={Uri.EscapeDataString(code)}");
        return Redirect($"/Panel/OAuthCallback?provider=onedrive&error={Uri.EscapeDataString(error_description ?? error ?? "Bilinmeyen hata")}");
    }

    [HttpPost("oauth/exchange-code")]
    public async Task<ActionResult> ExchangeCode([FromBody] OneDriveExchangeCodeDto dto)
    {
        if (!_oAuth.IsConfigured)
            return BadRequest(new { success = false, error = "OneDrive OAuth yapilandirilmamis" });

        var token = await _oAuth.ExchangeCodeAsync(dto.Code, dto.TenantId);
        if (token == null)
            return Ok(new { success = false, error = "Microsoft'tan token alinamadi. Kod gecersiz veya suresi dolmus olabilir." });

        var drives = await _oAuth.GetDrivesAsync(token.AccessToken);

        return Ok(new
        {
            success = true,
            refreshToken = token.RefreshToken,
            tenantId = dto.TenantId,
            drives = drives.Select(d => new
            {
                d.DriveId,
                d.Name,
                d.DriveType,
                d.OwnerName,
                d.TotalSpace,
                d.UsedSpace
            })
        });
    }

    [HttpPost("oauth/save")]
    public async Task<ActionResult> SaveOAuthAccount([FromBody] OneDriveOAuthSaveDto dto)
    {
        var account = new OneDriveAccount
        {
            Name = dto.Name,
            TenantId = dto.TenantId ?? "common",
            ClientId = "", // OAuth flow'da appsettings'ten gelir
            ClientSecret = "", // OAuth flow'da appsettings'ten gelir
            RefreshToken = dto.RefreshToken,
            DriveId = dto.DriveId,
            IsDefault = dto.IsDefault
        };

        var created = await _crud.CreateAccountAsync(account);
        return Ok(new { success = true, id = created.Id });
    }

    [HttpPost("test-connection")]
    public async Task<ActionResult> TestConnection()
    {
        var (success, error) = await _oneDriveService.TestConnectionAsync();
        return Ok(new { success, error });
    }
}

public class OneDriveExchangeCodeDto
{
    public string Code { get; set; } = "";
    public string? TenantId { get; set; }
}

public class OneDriveOAuthSaveDto
{
    public string Name { get; set; } = "";
    public string? TenantId { get; set; }
    public string RefreshToken { get; set; } = "";
    public string DriveId { get; set; } = "";
    public bool IsDefault { get; set; }
}
