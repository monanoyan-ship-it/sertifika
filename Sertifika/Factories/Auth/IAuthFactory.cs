namespace Sertifika.Factories.Auth;

public interface IAuthFactory
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<object?> GetCurrentUserAsync(int userId);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public object? User { get; set; }
    public string? Message { get; set; }
}
