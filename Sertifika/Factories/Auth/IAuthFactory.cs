namespace Sertifika.Factories.Auth;

public interface IAuthFactory
{
    Task<(bool Success, object Result)> LoginAsync(string email, string password);
    Task<(bool Success, object Result)> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<object?> GetCurrentUserAsync(int userId);
}
