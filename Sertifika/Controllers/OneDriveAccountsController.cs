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
        return Ok(accounts.Select(ToMasked));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetAccount(int id)
    {
        var a = await _crud.GetAccountAsync(id);
        if (a == null) return NotFound();
        return Ok(ToMasked(a));
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateAccount([FromBody] OneDriveAccountUpsertRequest req)
    {
        var created = await _crud.CreateAccountAsync(new OneDriveAccount
        {
            Name = req.Name,
            TenantId = req.TenantId ?? "common",
            ClientId = req.ClientId ?? "",
            ClientSecret = req.ClientSecret ?? "",
            DriveUserId = req.DriveUserId ?? "",
            DriveId = req.DriveId ?? "",
            BasePath = req.BasePath ?? "Sertifikalar",
            IsDefault = req.IsDefault,
            IsEnabled = req.IsEnabled
        });
        return CreatedAtAction(nameof(GetAccount), new { id = created.Id }, ToMasked(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] OneDriveAccountUpsertRequest req)
    {
        var existing = await _crud.GetAccountAsync(id);
        if (existing == null) return NotFound();

        var isOAuth = !string.IsNullOrEmpty(existing.RefreshToken);

        await _crud.UpdateAccountAsync(new OneDriveAccount
        {
            Id = id,
            Name = req.Name,
            TenantId = req.TenantId ?? "common",
            ClientId = req.ClientId ?? "",
            ClientSecret = req.ClientSecret ?? "",
            DriveUserId = req.DriveUserId ?? "",
            DriveId = req.DriveId ?? "",
            BasePath = req.BasePath ?? "Sertifikalar",
            IsDefault = req.IsDefault,
            IsEnabled = req.IsEnabled
        }, updateSecret: !isOAuth);
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _crud.SetDefaultAsync(id);
        return Ok(new { message = "Varsayilan hesap ayarlandi." });
    }

    [HttpPost("{id}/toggle-enabled")]
    public async Task<IActionResult> ToggleEnabled(int id, [FromBody] ToggleEnabledRequest req)
    {
        await _crud.SetEnabledAsync(id, req.Enabled);
        return NoContent();
    }

    [HttpPost("{id}/test-connection")]
    public async Task<ActionResult<object>> TestAccount(int id)
    {
        var result = await _oneDriveService.TestAccountAsync(id);
        await _crud.RecordTestResultAsync(id, result.Success, result.Error, result.QuotaTotalBytes, result.QuotaUsedBytes);
        return Ok(new
        {
            success = result.Success,
            error = result.Error,
            quotaTotal = result.QuotaTotalBytes,
            quotaUsed = result.QuotaUsedBytes,
            driveType = result.DriveType,
            ownerName = result.OwnerName
        });
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
            ClientId = "",
            ClientSecret = "",
            RefreshToken = dto.RefreshToken,
            DriveId = dto.DriveId,
            BasePath = string.IsNullOrWhiteSpace(dto.BasePath) ? "Sertifikalar" : dto.BasePath,
            IsDefault = dto.IsDefault
        };

        var created = await _crud.CreateAccountAsync(account);
        return Ok(new { success = true, id = created.Id });
    }

    [HttpPost("oauth/reconnect/{id}")]
    public async Task<ActionResult> Reconnect(int id, [FromBody] OneDriveReconnectDto dto)
    {
        var existing = await _crud.GetAccountAsync(id);
        if (existing == null) return NotFound();

        await _crud.UpdateOAuthTokensAsync(id, dto.RefreshToken, dto.DriveId);
        return Ok(new { success = true });
    }

    [HttpPost("test-connection")]
    public async Task<ActionResult> TestDefault()
    {
        var (success, error) = await _oneDriveService.TestConnectionAsync();
        return Ok(new { success, error });
    }

    private static object ToMasked(OneDriveAccount a) => new
    {
        a.Id,
        a.Name,
        a.TenantId,
        a.DriveId,
        a.DriveUserId,
        a.BasePath,
        a.IsDefault,
        a.IsEnabled,
        a.LastTestedAt,
        a.LastTestStatus,
        a.LastTestError,
        a.QuotaTotalBytes,
        a.QuotaUsedBytes,
        a.QuotaCheckedAt,
        a.CreatedAt,
        HasRefreshToken = !string.IsNullOrEmpty(a.RefreshToken),
        HasClientSecret = !string.IsNullOrEmpty(a.ClientSecret)
    };
}

public class OneDriveAccountUpsertRequest
{
    public string Name { get; set; } = "";
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? DriveUserId { get; set; }
    public string? DriveId { get; set; }
    public string? BasePath { get; set; }
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; } = true;
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
    public string? BasePath { get; set; }
    public bool IsDefault { get; set; }
}

public class OneDriveReconnectDto
{
    public string RefreshToken { get; set; } = "";
    public string DriveId { get; set; } = "";
}
