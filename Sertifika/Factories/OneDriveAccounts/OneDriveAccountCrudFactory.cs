using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.OneDriveAccounts;

public class OneDriveAccountCrudFactory : IOneDriveAccountCrudFactory
{
    private readonly IOneDriveAccountEntityService _accountService;
    private readonly IUnitOfWork _unitOfWork;

    public OneDriveAccountCrudFactory(IOneDriveAccountEntityService accountService, IUnitOfWork unitOfWork)
    {
        _accountService = accountService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<OneDriveAccount>> GetAccountsAsync()
        => await _accountService.GetActiveAccountsAsync();

    public async Task<OneDriveAccount?> GetAccountAsync(int id)
        => await _accountService.GetByIdAsync(id);

    public async Task<OneDriveAccount> CreateAccountAsync(OneDriveAccount account)
    {
        // If first account or marked as default, ensure only one default
        if (account.IsDefault)
            await ClearDefaultAsync();

        _accountService.Add(account);
        await _unitOfWork.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAccountAsync(OneDriveAccount account)
    {
        var existing = await _accountService.GetByIdAsync(account.Id);
        if (existing == null) return;

        existing.Name = account.Name;
        existing.TenantId = account.TenantId;
        existing.ClientId = account.ClientId;
        existing.ClientSecret = account.ClientSecret;
        existing.DriveUserId = account.DriveUserId;
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
