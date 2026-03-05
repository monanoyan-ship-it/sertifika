using Sertifika.Entities;

namespace Sertifika.Factories.Templates;

public interface ITemplateCrudFactory
{
    Task<IEnumerable<CertificateTemplate>> GetTemplatesAsync();
    Task<CertificateTemplate?> GetTemplateAsync(int id);
    Task<CertificateTemplate> CreateTemplateAsync(CertificateTemplate template);
    Task UpdateTemplateAsync(CertificateTemplate template);
    Task<bool> DeleteTemplateAsync(int id);
}
