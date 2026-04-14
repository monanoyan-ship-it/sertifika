using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class ContactEntityService : IContactEntityService
{
    private readonly AppDbContext _context;

    public ContactEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contact>> GetByCompanyIdAsync(int companyId)
        => await _context.Contacts
            .Where(c => c.IsActive && c.CompanyId == companyId)
            .OrderBy(c => c.FirstName)
            .ToListAsync();

    public async Task<Contact?> GetByIdAsync(int id)
        => await _context.Contacts.FindAsync(id);

    public async Task<Contact?> FindByCompanyAndEmailAsync(int companyId, string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Email != null && c.Email.ToLower() == normalized);
    }

    public void Add(Contact contact) => _context.Contacts.Add(contact);
    public void AddRange(IEnumerable<Contact> contacts) => _context.Contacts.AddRange(contacts);
    public void Update(Contact contact) => _context.Entry(contact).State = EntityState.Modified;
}
