using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.SmtpAccounts;

public class SmtpAccountCrudFactory : ISmtpAccountCrudFactory
{
    private readonly ISmtpAccountEntityService _accountService;
    private readonly IUnitOfWork _unitOfWork;

    public SmtpAccountCrudFactory(ISmtpAccountEntityService accountService, IUnitOfWork unitOfWork)
    {
        _accountService = accountService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SmtpAccount>> GetAccountsAsync()
        => await _accountService.GetActiveAccountsAsync();

    public async Task<SmtpAccount?> GetAccountAsync(int id)
        => await _accountService.GetByIdAsync(id);

    public async Task<SmtpAccount> CreateAccountAsync(SmtpAccount account)
    {
        if (account.IsDefault)
            await ClearDefaultAsync();

        _accountService.Add(account);
        await _unitOfWork.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAccountAsync(SmtpAccount account)
    {
        var existing = await _accountService.GetByIdAsync(account.Id);
        if (existing == null) return;

        existing.Name = account.Name;
        existing.Host = account.Host;
        existing.Port = account.Port;
        existing.Username = account.Username;
        existing.Password = account.Password;
        existing.FromEmail = account.FromEmail;
        existing.FromName = account.FromName;
        existing.UseSsl = account.UseSsl;
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
            a.IsDefault = false;
    }
}
