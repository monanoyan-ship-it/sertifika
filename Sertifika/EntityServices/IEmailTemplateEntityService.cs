using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface IEmailTemplateEntityService
{
    Task<IEnumerable<EmailTemplate>> GetActiveTemplatesAsync();
    Task<EmailTemplate?> GetByIdAsync(int id);
    Task<EmailTemplate?> GetDefaultTemplateAsync();
    void Add(EmailTemplate template);
    void Update(EmailTemplate template);
}
