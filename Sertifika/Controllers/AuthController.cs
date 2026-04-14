using System.Security.Claims;
using Sertifika.Factories.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string AuthCookieName = "auth_token";
    private readonly IAuthFactory _auth;

    public AuthController(IAuthFactory auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, result) = await _auth.LoginAsync(request.Email, request.Password);
        if (!success) return Unauthorized(result);

        SetAuthCookie(result);
        return Ok(StripToken(result));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, result) = await _auth.RegisterAsync(request.FirstName, request.LastName, request.Email, request.Password);
        if (!success) return BadRequest(result);

        SetAuthCookie(result);
        return Ok(StripToken(result));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
        return Ok(new { message = "Cikis yapildi." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        var user = await _auth.GetCurrentUserAsync(int.Parse(userIdClaim));
        if (user == null) return NotFound();
        return Ok(user);
    }

    private void SetAuthCookie(object result)
    {
        var token = result.GetType().GetProperty("token")?.GetValue(result) as string;
        if (string.IsNullOrEmpty(token)) return;

        Response.Cookies.Append(AuthCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    private static object StripToken(object result)
    {
        var userProp = result.GetType().GetProperty("user")?.GetValue(result);
        return new { user = userProp };
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
