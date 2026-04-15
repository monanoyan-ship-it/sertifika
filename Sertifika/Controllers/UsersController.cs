using System.Security.Claims;
using Sertifika.Factories.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserCrudFactory _crud;

    public UsersController(IUserCrudFactory crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
        => Ok(await _crud.GetUsersAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _crud.GetUserAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
    {
        try
        {
            var created = await _crud.CreateUserAsync(request);
            return Ok(created);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
    {
        try
        {
            await _crud.UpdateUserAsync(id, request);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id}/password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            await _crud.ChangePasswordAsync(id, request.NewPassword);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var ok = await _crud.DeleteUserAsync(id, currentUserId);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("roles")]
    public IActionResult GetRoles()
        => Ok(new[]
        {
            new { value = "Admin", label = "Admin - Tam yetki (kullanici yonetimi dahil)" },
            new { value = "CertificateCreator", label = "Operator - Sertifika uretebilir, duzenleyebilir" },
            new { value = "Viewer", label = "Goruntuleyici - Sadece okuma" }
        });
}

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
