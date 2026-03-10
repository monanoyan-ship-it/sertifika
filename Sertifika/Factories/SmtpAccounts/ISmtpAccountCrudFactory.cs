using Sertifika.Entities;

namespace Sertifika.Factories.SmtpAccounts;

public interface ISmtpAccountCrudFactory
{
    Task<IEnumerable<SmtpAccount>> GetAccountsAsync();
    Task<SmtpAccount?> GetAccountAsync(int id);
    Task<SmtpAccount> CreateAccountAsync(SmtpAccount account);
    Task UpdateAccountAsync(SmtpAccount account);
    Task SetDefaultAsync(int id);
    Task<bool> DeleteAccountAsync(int id);
}
