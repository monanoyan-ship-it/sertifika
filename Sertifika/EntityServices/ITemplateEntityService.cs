using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface ITemplateEntityService
{
    Task<IEnumerable<CertificateTemplate>> GetActiveTemplatesAsync();
    Task<CertificateTemplate?> GetByIdAsync(int id);
    void Add(CertificateTemplate template);
    void Update(CertificateTemplate template);
}
