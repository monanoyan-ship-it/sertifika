using Sertifika.Entities;

namespace Sertifika.Factories.Users;

public interface IUserCrudFactory
{
    Task<IEnumerable<object>> GetUsersAsync();
    Task<object?> GetUserAsync(int id);
    Task<object> CreateUserAsync(UserCreateRequest request);
    Task UpdateUserAsync(int id, UserUpdateRequest request);
    Task ChangePasswordAsync(int id, string newPassword);
    Task<bool> DeleteUserAsync(int id, int currentUserId);
}

public class UserCreateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
}

public class UserUpdateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
}
