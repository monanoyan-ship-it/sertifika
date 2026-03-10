using Sertifika.Entities;

namespace Sertifika.Factories.EmailTemplates;

public interface IEmailTemplateCrudFactory
{
    Task<IEnumerable<EmailTemplate>> GetTemplatesAsync();
    Task<EmailTemplate?> GetTemplateAsync(int id);
    Task<EmailTemplate?> GetDefaultTemplateAsync();
    Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template);
    Task UpdateTemplateAsync(EmailTemplate template);
    Task SetDefaultAsync(int id);
    Task<bool> DeleteTemplateAsync(int id);
}
