using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class SignatureEntityService : ISignatureEntityService
{
    private readonly AppDbContext _context;

    public SignatureEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Signature>> GetActiveSignaturesAsync()
        => await _context.Signatures.Where(s => s.IsActive).ToListAsync();

    public async Task<Signature?> GetByIdAsync(int id)
        => await _context.Signatures.FindAsync(id);

    public void Add(Signature signature) => _context.Signatures.Add(signature);

    public void Update(Signature signature) => _context.Entry(signature).State = EntityState.Modified;
}
