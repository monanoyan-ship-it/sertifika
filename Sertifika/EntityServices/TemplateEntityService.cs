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
        => await _context.CertificateTemplates
            .Where(t => t.IsActive)
            .Include(t => t.TemplateSignatures.Where(ts => ts.IsActive))
                .ThenInclude(ts => ts.Signature)
            .ToListAsync();

    public async Task<CertificateTemplate?> GetByIdAsync(int id)
        => await _context.CertificateTemplates.FindAsync(id);

    public async Task<CertificateTemplate?> GetByIdWithSignaturesAsync(int id)
        => await _context.CertificateTemplates
            .Include(t => t.TemplateSignatures.Where(ts => ts.IsActive))
                .ThenInclude(ts => ts.Signature)
            .FirstOrDefaultAsync(t => t.Id == id);

    public void Add(CertificateTemplate template) => _context.CertificateTemplates.Add(template);

    public void Update(CertificateTemplate template) => _context.Entry(template).State = EntityState.Modified;

    public void AddTemplateSignature(TemplateSignature ts) => _context.TemplateSignatures.Add(ts);

    public void RemoveTemplateSignatures(IEnumerable<TemplateSignature> signatures)
    {
        foreach (var sig in signatures)
        {
            sig.IsActive = false;
        }
    }
}
