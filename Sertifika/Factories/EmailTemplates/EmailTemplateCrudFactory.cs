using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.EmailTemplates;

public class EmailTemplateCrudFactory : IEmailTemplateCrudFactory
{
    private readonly IEmailTemplateEntityService _templateService;
    private readonly IUnitOfWork _unitOfWork;

    public EmailTemplateCrudFactory(IEmailTemplateEntityService templateService, IUnitOfWork unitOfWork)
    {
        _templateService = templateService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<EmailTemplate>> GetTemplatesAsync()
        => await _templateService.GetActiveTemplatesAsync();

    public async Task<EmailTemplate?> GetTemplateAsync(int id)
        => await _templateService.GetByIdAsync(id);

    public async Task<EmailTemplate?> GetDefaultTemplateAsync()
        => await _templateService.GetDefaultTemplateAsync();

    public async Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template)
    {
        if (template.IsDefault)
            await ClearDefaultAsync();

        _templateService.Add(template);
        await _unitOfWork.SaveChangesAsync();
        return template;
    }

    public async Task UpdateTemplateAsync(EmailTemplate template)
    {
        var existing = await _templateService.GetByIdAsync(template.Id);
        if (existing == null) return;

        if (template.IsDefault && !existing.IsDefault)
            await ClearDefaultAsync();

        existing.Name = template.Name;
        existing.Subject = template.Subject;
        existing.Body = template.Body;
        existing.IsDefault = template.IsDefault;
        existing.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetDefaultAsync(int id)
    {
        await ClearDefaultAsync();

        var template = await _templateService.GetByIdAsync(id);
        if (template == null) return;

        template.IsDefault = true;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteTemplateAsync(int id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null) return false;

        template.IsActive = false;
        template.IsDefault = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task ClearDefaultAsync()
    {
        var templates = await _templateService.GetActiveTemplatesAsync();
        foreach (var t in templates.Where(t => t.IsDefault))
            t.IsDefault = false;
    }
}
