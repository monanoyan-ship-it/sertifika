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

    public async Task<Company?> FindByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var normalized = name.Trim().ToLowerInvariant();
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.IsActive && c.Name.ToLower() == normalized);
    }

    public void Add(Company company) => _context.Companies.Add(company);

    public void Update(Company company) => _context.Entry(company).State = EntityState.Modified;
}
