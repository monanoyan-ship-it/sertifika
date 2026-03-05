using Sertifika.Entities;

namespace Sertifika.Factories.OneDriveAccounts;

public interface IOneDriveAccountCrudFactory
{
    Task<IEnumerable<OneDriveAccount>> GetAccountsAsync();
    Task<OneDriveAccount?> GetAccountAsync(int id);
    Task<OneDriveAccount> CreateAccountAsync(OneDriveAccount account);
    Task UpdateAccountAsync(OneDriveAccount account);
    Task SetDefaultAsync(int id);
    Task<bool> DeleteAccountAsync(int id);
}
