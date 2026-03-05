using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class CompanyEntityService : ICompanyEntityService
{
    private readonly AppDbContext _context;

    public CompanyEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Company>> GetActiveCompaniesAsync()
        => await _context.Companies.Where(c => c.IsActive).ToListAsync();

    public async Task<Company?> GetByIdAsync(int id)
        => await _context.Companies.FindAsync(id);

    public void Add(Company company) => _context.Companies.Add(company);

    public void Update(Company company) => _context.Entry(company).State = EntityState.Modified;
}
