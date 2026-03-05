using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class TemplateEntityService : ITemplateEntityService
{
    private readonly AppDbContext _context;

    public TemplateEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CertificateTemplate>> GetActiveTemplatesAsync()
        => await _context.CertificateTemplates.Where(t => t.IsActive).ToListAsync();

    public async Task<CertificateTemplate?> GetByIdAsync(int id)
        => await _context.CertificateTemplates.FindAsync(id);

    public void Add(CertificateTemplate template) => _context.CertificateTemplates.Add(template);

    public void Update(CertificateTemplate template) => _context.Entry(template).State = EntityState.Modified;
}
