using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface ICompanyEntityService
{
    Task<IEnumerable<Company>> GetActiveCompaniesAsync();
    Task<Company?> GetByIdAsync(int id);
    Task<Company?> FindByNameAsync(string name);
    void Add(Company company);
    void Update(Company company);
}
