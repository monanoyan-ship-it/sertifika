using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class EmailTemplateEntityService : IEmailTemplateEntityService
{
    private readonly AppDbContext _context;

    public EmailTemplateEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmailTemplate>> GetActiveTemplatesAsync()
        => await _context.EmailTemplates.Where(t => t.IsActive).ToListAsync();

    public async Task<EmailTemplate?> GetByIdAsync(int id)
        => await _context.EmailTemplates.FindAsync(id);

    public async Task<EmailTemplate?> GetDefaultTemplateAsync()
        => await _context.EmailTemplates.FirstOrDefaultAsync(t => t.IsActive && t.IsDefault);

    public void Add(EmailTemplate template) => _context.EmailTemplates.Add(template);

    public void Update(EmailTemplate template) => _context.Entry(template).State = EntityState.Modified;
}
