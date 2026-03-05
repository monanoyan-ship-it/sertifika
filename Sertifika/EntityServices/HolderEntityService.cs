using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class HolderEntityService : IHolderEntityService
{
    private readonly AppDbContext _context;

    public HolderEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Holder>> GetActiveHoldersAsync()
        => await _context.Holders.Where(h => h.IsActive).ToListAsync();

    public async Task<Holder?> GetByIdAsync(int id)
        => await _context.Holders.FindAsync(id);

    public async Task<Holder?> GetByEmailAsync(string email)
        => await _context.Holders.FirstOrDefaultAsync(h => h.Email == email);

    public void Add(Holder holder) => _context.Holders.Add(holder);

    public void Update(Holder holder) => _context.Entry(holder).State = EntityState.Modified;
}
