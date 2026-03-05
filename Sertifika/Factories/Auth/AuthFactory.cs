using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;
using Sertifika.Services;

namespace Sertifika.Factories.Auth;

public class AuthFactory : IAuthFactory
{
    private readonly IUserEntityService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public AuthFactory(IUserEntityService userService, IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<(bool Success, object Result)> LoginAsync(string email, string password)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
            return (false, new { message = "E-posta veya sifre hatali." });

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, new { message = "E-posta veya sifre hatali." });

        var token = _jwtService.GenerateToken(user);

        return (true, new
        {
            token,
            user = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                Role = user.Role.ToString()
            }
        });
    }

    public async Task<(bool Success, object Result)> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        var existing = await _userService.GetByEmailAsync(email);
        if (existing != null)
            return (false, new { message = "Bu e-posta adresi zaten kayitli." });

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Viewer
        };

        _userService.Add(user);
        await _unitOfWork.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return (true, new
        {
            token,
            user = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                Role = user.Role.ToString()
            }
        });
    }

    public async Task<object?> GetCurrentUserAsync(int userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return null;

        return new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            Role = user.Role.ToString()
        };
    }
}
