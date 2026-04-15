using Sertifika.Entities;

namespace Sertifika.Factories.OneDriveAccounts;

public interface IOneDriveAccountCrudFactory
{
    Task<IEnumerable<OneDriveAccount>> GetAccountsAsync();
    Task<OneDriveAccount?> GetAccountAsync(int id);
    Task<OneDriveAccount> CreateAccountAsync(OneDriveAccount account);
    Task UpdateAccountAsync(OneDriveAccount account, bool updateSecret);
    Task SetDefaultAsync(int id);
    Task SetEnabledAsync(int id, bool enabled);
    Task RecordTestResultAsync(int id, bool success, string? error, long? quotaTotal, long? quotaUsed);
    Task UpdateOAuthTokensAsync(int id, string refreshToken, string driveId);
    Task<bool> DeleteAccountAsync(int id);
}
