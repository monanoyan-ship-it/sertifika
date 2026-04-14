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

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return new AuthResult { Success = false, Message = "E-posta veya sifre hatali." };

        return new AuthResult
        {
            Success = true,
            Token = _jwtService.GenerateToken(user),
            User = BuildUserDto(user)
        };
    }

    public async Task<AuthResult> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        var existing = await _userService.GetByEmailAsync(email);
        if (existing != null)
            return new AuthResult { Success = false, Message = "Bu e-posta adresi zaten kayitli." };

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

        return new AuthResult
        {
            Success = true,
            Token = _jwtService.GenerateToken(user),
            User = BuildUserDto(user)
        };
    }

    public async Task<object?> GetCurrentUserAsync(int userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        return user == null ? null : BuildUserDto(user);
    }

    private static object BuildUserDto(User user) => new
    {
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        Role = user.Role.ToString()
    };
}
