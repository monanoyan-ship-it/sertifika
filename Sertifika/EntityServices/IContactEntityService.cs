using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface IContactEntityService
{
    Task<IEnumerable<Contact>> GetByCompanyIdAsync(int companyId);
    Task<Contact?> GetByIdAsync(int id);
    Task<Contact?> FindByCompanyAndEmailAsync(int companyId, string email);
    void Add(Contact contact);
    void AddRange(IEnumerable<Contact> contacts);
    void Update(Contact contact);
}
