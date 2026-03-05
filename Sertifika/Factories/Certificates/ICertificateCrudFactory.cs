using Sertifika.Entities;

namespace Sertifika.Factories.Certificates;

public interface ICertificateCrudFactory
{
    Task<IEnumerable<Certificate>> GetCertificatesAsync();
    Task<Certificate?> GetCertificateAsync(int id);
    Task<IEnumerable<Certificate>> GetCertificatesByHolderAsync(int holderId);
    Task<IEnumerable<Certificate>> GetCertificatesByCategoryAsync(int categoryId);
    Task<Certificate> CreateCertificateAsync(Certificate certificate);
    Task UpdateCertificateAsync(Certificate certificate);
    Task<bool> DeleteCertificateAsync(int id);
}
