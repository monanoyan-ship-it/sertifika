using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;
using Sertifika.Services;

namespace Sertifika.Factories.OneDriveAccounts;

public class OneDriveAccountCrudFactory : IOneDriveAccountCrudFactory
{
    private readonly IOneDriveAccountEntityService _accountService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EncryptionService _crypto;

    public OneDriveAccountCrudFactory(
        IOneDriveAccountEntityService accountService,
        IUnitOfWork unitOfWork,
        EncryptionService crypto)
    {
        _accountService = accountService;
        _unitOfWork = unitOfWork;
        _crypto = crypto;
    }

    public async Task<IEnumerable<OneDriveAccount>> GetAccountsAsync()
        => await _accountService.GetActiveAccountsAsync();

    public async Task<OneDriveAccount?> GetAccountAsync(int id)
        => await _accountService.GetByIdAsync(id);

    public async Task<OneDriveAccount> CreateAccountAsync(OneDriveAccount account)
    {
        var existing = (await _accountService.GetActiveAccountsAsync()).ToList();

        if (existing.Count == 0 || !existing.Any(a => a.IsDefault))
            account.IsDefault = true;

        if (account.IsDefault)
            await ClearDefaultAsync();

        // Hassas alanlari sifrele
        if (!string.IsNullOrEmpty(account.ClientSecret))
            account.ClientSecret = _crypto.Encrypt(account.ClientSecret);
        if (!string.IsNullOrEmpty(account.RefreshToken))
            account.RefreshToken = _crypto.Encrypt(account.RefreshToken);

        if (string.IsNullOrWhiteSpace(account.BasePath))
            account.BasePath = "Sertifikalar";

        _accountService.Add(account);
        await _unitOfWork.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAccountAsync(OneDriveAccount account, bool updateSecret)
    {
        var existing = await _accountService.GetByIdAsync(account.Id);
        if (existing == null) return;

        if (account.IsDefault && !existing.IsDefault)
            await ClearDefaultAsync();

        existing.Name = account.Name;
        existing.TenantId = account.TenantId;
        existing.DriveId = account.DriveId;
        existing.BasePath = string.IsNullOrWhiteSpace(account.BasePath) ? "Sertifikalar" : account.BasePath.Trim().Trim('/');
        existing.IsDefault = account.IsDefault;
        existing.IsEnabled = account.IsEnabled;

        if (updateSecret)
        {
            existing.ClientId = account.ClientId;
            if (!string.IsNullOrEmpty(account.ClientSecret))
                existing.ClientSecret = _crypto.Encrypt(account.ClientSecret);
            existing.DriveUserId = account.DriveUserId;
        }

        existing.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetDefaultAsync(int id)
    {
        await ClearDefaultAsync();

        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return;

        account.IsDefault = true;
        account.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetEnabledAsync(int id, bool enabled)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return;

        account.IsEnabled = enabled;
        if (!enabled) account.IsDefault = false;
        account.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RecordTestResultAsync(int id, bool success, string? error, long? quotaTotal, long? quotaUsed)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return;

        account.LastTestedAt = DateTime.UtcNow;
        account.LastTestStatus = success ? "success" : "failed";
        account.LastTestError = success ? null : (error ?? "Bilinmeyen hata");

        if (success && quotaTotal.HasValue)
        {
            account.QuotaTotalBytes = quotaTotal;
            account.QuotaUsedBytes = quotaUsed;
            account.QuotaCheckedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateOAuthTokensAsync(int id, string refreshToken, string driveId)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return;

        account.RefreshToken = _crypto.Encrypt(refreshToken);
        account.DriveId = driveId;
        account.IsEnabled = true;
        account.LastTestStatus = null;
        account.LastTestError = null;
        account.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteAccountAsync(int id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return false;

        account.IsActive = false;
        account.IsDefault = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task ClearDefaultAsync()
    {
        var accounts = await _accountService.GetActiveAccountsAsync();
        foreach (var a in accounts.Where(a => a.IsDefault))
        {
            a.IsDefault = false;
        }
    }
}
