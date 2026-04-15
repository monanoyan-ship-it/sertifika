using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Users;

public class UserCrudFactory : IUserCrudFactory
{
    private readonly IUserEntityService _userService;
    private readonly IUnitOfWork _unitOfWork;

    public UserCrudFactory(IUserEntityService userService, IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<object>> GetUsersAsync()
    {
        var users = await _userService.GetAllAsync();
        return users.Select(Project);
    }

    public async Task<object?> GetUserAsync(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user == null || !user.IsActive ? null : Project(user);
    }

    public async Task<object> CreateUserAsync(UserCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("E-posta gerekli.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ArgumentException("Sifre en az 6 karakter olmali.");

        var existing = await _userService.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException("Bu e-posta adresi zaten kayitli.");

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.Trim(),
            Role = request.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _userService.Add(user);
        await _unitOfWork.SaveChangesAsync();
        return Project(user);
    }

    public async Task UpdateUserAsync(int id, UserUpdateRequest request)
    {
        var user = await _userService.GetByIdAsync(id) ?? throw new ArgumentException("Kullanici bulunamadi.");
        if (!user.IsActive) throw new ArgumentException("Kullanici bulunamadi.");

        // Prevent removing the last admin
        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin)
        {
            var otherAdmins = await _userService.CountActiveAdminsAsync(excludeId: id);
            if (otherAdmins == 0)
                throw new InvalidOperationException("Sistemde en az bir Admin olmak zorunda.");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email.Trim();
        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;
        _userService.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new ArgumentException("Sifre en az 6 karakter olmali.");

        var user = await _userService.GetByIdAsync(id) ?? throw new ArgumentException("Kullanici bulunamadi.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _userService.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteUserAsync(int id, int currentUserId)
    {
        if (id == currentUserId)
            throw new InvalidOperationException("Kendi hesabinizi silemezsiniz.");

        var user = await _userService.GetByIdAsync(id);
        if (user == null || !user.IsActive) return false;

        if (user.Role == UserRole.Admin)
        {
            var otherAdmins = await _userService.CountActiveAdminsAsync(excludeId: id);
            if (otherAdmins == 0)
                throw new InvalidOperationException("Son Admin silinemez.");
        }

        var ok = await _userService.DeleteAsync(id);
        if (ok) await _unitOfWork.SaveChangesAsync();
        return ok;
    }

    private static object Project(User u) => new
    {
        u.Id,
        u.FirstName,
        u.LastName,
        u.Email,
        Role = u.Role.ToString(),
        u.CreatedAt
    };
}
