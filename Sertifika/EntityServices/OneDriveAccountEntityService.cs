using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class OneDriveAccountEntityService : IOneDriveAccountEntityService
{
    private readonly AppDbContext _context;

    public OneDriveAccountEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OneDriveAccount>> GetActiveAccountsAsync()
        => await _context.OneDriveAccounts.Where(a => a.IsActive).ToListAsync();

    public async Task<OneDriveAccount?> GetByIdAsync(int id)
        => await _context.OneDriveAccounts.FindAsync(id);

    public async Task<OneDriveAccount?> GetDefaultAccountAsync()
        => await _context.OneDriveAccounts.FirstOrDefaultAsync(a => a.IsActive && a.IsDefault);

    public void Add(OneDriveAccount account) => _context.OneDriveAccounts.Add(account);

    public void Update(OneDriveAccount account) => _context.Entry(account).State = EntityState.Modified;
}
