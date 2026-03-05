using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface ICertificateEntityService
{
    Task<IEnumerable<Certificate>> GetActiveCertificatesAsync();
    Task<Certificate?> GetByIdAsync(int id);
    Task<Certificate?> GetByNumberAsync(string certificateNumber);
    Task<IEnumerable<Certificate>> GetByHolderIdAsync(int holderId);
    Task<IEnumerable<Certificate>> GetByCategoryIdAsync(int categoryId);
    void Add(Certificate certificate);
    void Update(Certificate certificate);
}
