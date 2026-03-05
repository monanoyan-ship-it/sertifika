using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class CertificateEntityService : ICertificateEntityService
{
    private readonly AppDbContext _context;

    public CertificateEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Certificate>> GetActiveCertificatesAsync()
        => await _context.Certificates
            .Include(c => c.Holder)
            .Include(c => c.Category)
            .Where(c => c.IsActive)
            .ToListAsync();

    public async Task<Certificate?> GetByIdAsync(int id)
        => await _context.Certificates
            .Include(c => c.Holder)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Certificate?> GetByNumberAsync(string certificateNumber)
        => await _context.Certificates
            .Include(c => c.Holder)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber);

    public async Task<IEnumerable<Certificate>> GetByHolderIdAsync(int holderId)
        => await _context.Certificates
            .Include(c => c.Category)
            .Where(c => c.IsActive && c.HolderId == holderId)
            .ToListAsync();

    public async Task<IEnumerable<Certificate>> GetByCategoryIdAsync(int categoryId)
        => await _context.Certificates
            .Include(c => c.Holder)
            .Where(c => c.IsActive && c.CategoryId == categoryId)
            .ToListAsync();

    public void Add(Certificate certificate) => _context.Certificates.Add(certificate);

    public void Update(Certificate certificate) => _context.Entry(certificate).State = EntityState.Modified;
}
