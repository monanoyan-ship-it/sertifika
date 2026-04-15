using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;
using Sertifika.Services;

namespace Sertifika.Factories.SmtpAccounts;

public class SmtpAccountCrudFactory : ISmtpAccountCrudFactory
{
    private readonly ISmtpAccountEntityService _accountService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SmtpMailDispatcher _dispatcher;

    public SmtpAccountCrudFactory(
        ISmtpAccountEntityService accountService,
        IUnitOfWork unitOfWork,
        SmtpMailDispatcher dispatcher)
    {
        _accountService = accountService;
        _unitOfWork = unitOfWork;
        _dispatcher = dispatcher;
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

    public async Task UpdateAccountAsync(SmtpAccount account, bool updatePassword)
    {
        var existing = await _accountService.GetByIdAsync(account.Id);
        if (existing == null) return;

        if (account.IsDefault && !existing.IsDefault)
            await ClearDefaultAsync();

        existing.Name = account.Name;
        existing.Host = account.Host;
        existing.Port = account.Port;
        existing.Username = account.Username;
        if (updatePassword && !string.IsNullOrEmpty(account.Password))
            existing.Password = account.Password;
        existing.FromEmail = account.FromEmail;
        existing.FromName = account.FromName;
        existing.UseSsl = account.UseSsl;
        existing.UseOAuth = account.UseOAuth;
        existing.UseGraphApi = account.UseGraphApi;
        existing.TenantId = account.TenantId;
        existing.ClientId = account.ClientId;
        // Client secret'i de password gibi bos birakildiginda mevcut korunur.
        if (updatePassword && !string.IsNullOrEmpty(account.ClientSecret))
            existing.ClientSecret = account.ClientSecret;
        existing.IsDefault = account.IsDefault;
        existing.IsEnabled = account.IsEnabled;
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

    public async Task RecordTestResultAsync(int id, bool success, string? error)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return;

        account.LastTestedAt = DateTime.UtcNow;
        account.LastTestStatus = success ? "success" : "failed";
        account.LastTestError = success ? null : (error ?? "Bilinmeyen hata");
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

    public async Task<SmtpTestResult> TestConnectionAsync(int id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return new SmtpTestResult { Success = false, Error = "Hesap bulunamadi" };

        return await RunTestAsync(id, account, account.FromEmail, probe: true);
    }

    public async Task<SmtpTestResult> SendTestEmailAsync(int id, string toEmail)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null) return new SmtpTestResult { Success = false, Error = "Hesap bulunamadi" };

        return await RunTestAsync(id, account, toEmail, probe: false);
    }

    private async Task<SmtpTestResult> RunTestAsync(int id, SmtpAccount account, string toEmail, bool probe)
    {
        try
        {
            var subject = probe ? "[Sertifika] SMTP baglanti testi" : $"SMTP Test - {account.Name}";
            var body = probe
                ? "<p>Bu mail, panelden baslatilan SMTP baglanti testinin sonucudur.</p>"
                : $"""
                    <h3>SMTP baglanti testi basarili.</h3>
                    <p><strong>{account.Name}</strong> hesabindan, sertifika panelinden gonderildi.</p>
                    <p><small>Gonderim zamani: {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC</small></p>
                    """;

            var mode = account.UseGraphApi ? "Graph API" : (account.UseOAuth ? "SMTP OAuth" : "Basic SMTP");
            body = $"<p><em>Mod: {mode}</em></p>" + body;

            await _dispatcher.SendAsync(account, new EmailMessage
            {
                ToEmail = toEmail,
                Subject = subject,
                HtmlBody = body
            });

            await RecordTestResultAsync(id, true, null);
            return new SmtpTestResult { Success = true };
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            await RecordTestResultAsync(id, false, msg);
            return new SmtpTestResult { Success = false, Error = msg };
        }
    }

    private async Task ClearDefaultAsync()
    {
        var accounts = await _accountService.GetActiveAccountsAsync();
        foreach (var a in accounts.Where(a => a.IsDefault))
            a.IsDefault = false;
    }
}
