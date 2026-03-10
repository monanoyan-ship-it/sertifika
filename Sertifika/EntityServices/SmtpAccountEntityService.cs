using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class SmtpAccountEntityService : ISmtpAccountEntityService
{
    private readonly AppDbContext _context;

    public SmtpAccountEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SmtpAccount>> GetActiveAccountsAsync()
        => await _context.SmtpAccounts.Where(a => a.IsActive).ToListAsync();

    public async Task<SmtpAccount?> GetByIdAsync(int id)
        => await _context.SmtpAccounts.FindAsync(id);

    public async Task<SmtpAccount?> GetDefaultAccountAsync()
        => await _context.SmtpAccounts.FirstOrDefaultAsync(a => a.IsActive && a.IsDefault);

    public void Add(SmtpAccount account) => _context.SmtpAccounts.Add(account);

    public void Update(SmtpAccount account) => _context.Entry(account).State = EntityState.Modified;
}
