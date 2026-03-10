using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface ISmtpAccountEntityService
{
    Task<IEnumerable<SmtpAccount>> GetActiveAccountsAsync();
    Task<SmtpAccount?> GetByIdAsync(int id);
    Task<SmtpAccount?> GetDefaultAccountAsync();
    void Add(SmtpAccount account);
    void Update(SmtpAccount account);
}
