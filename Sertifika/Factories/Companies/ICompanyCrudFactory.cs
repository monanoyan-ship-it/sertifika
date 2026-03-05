using Sertifika.Entities;

namespace Sertifika.Factories.Companies;

public interface ICompanyCrudFactory
{
    Task<IEnumerable<Company>> GetCompaniesAsync();
    Task<Company?> GetCompanyAsync(int id);
    Task<Company> CreateCompanyAsync(Company company);
    Task UpdateCompanyAsync(Company company);
    Task<bool> DeleteCompanyAsync(int id);
}
