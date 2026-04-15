using Sertifika.Entities;

namespace Sertifika.Factories.SmtpAccounts;

public interface ISmtpAccountCrudFactory
{
    Task<IEnumerable<SmtpAccount>> GetAccountsAsync();
    Task<SmtpAccount?> GetAccountAsync(int id);
    Task<SmtpAccount> CreateAccountAsync(SmtpAccount account);
    Task UpdateAccountAsync(SmtpAccount account, bool updatePassword);
    Task SetDefaultAsync(int id);
    Task SetEnabledAsync(int id, bool enabled);
    Task RecordTestResultAsync(int id, bool success, string? error);
    Task<bool> DeleteAccountAsync(int id);
    Task<SmtpTestResult> TestConnectionAsync(int id);
    Task<SmtpTestResult> SendTestEmailAsync(int id, string toEmail);
}

public class SmtpTestResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}
