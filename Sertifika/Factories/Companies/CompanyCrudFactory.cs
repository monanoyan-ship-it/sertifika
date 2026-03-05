using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Companies;

public class CompanyCrudFactory : ICompanyCrudFactory
{
    private readonly ICompanyEntityService _companyService;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyCrudFactory(ICompanyEntityService companyService, IUnitOfWork unitOfWork)
    {
        _companyService = companyService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Company>> GetCompaniesAsync()
    {
        return await _companyService.GetActiveCompaniesAsync();
    }

    public async Task<Company?> GetCompanyAsync(int id)
    {
        return await _companyService.GetByIdAsync(id);
    }

    public async Task<Company> CreateCompanyAsync(Company company)
    {
        _companyService.Add(company);
        await _unitOfWork.SaveChangesAsync();
        return company;
    }

    public async Task UpdateCompanyAsync(Company company)
    {
        company.UpdatedAt = DateTime.UtcNow;
        _companyService.Update(company);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteCompanyAsync(int id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null) return false;

        company.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
