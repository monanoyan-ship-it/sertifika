using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Templates;

public class TemplateCrudFactory : ITemplateCrudFactory
{
    private readonly ITemplateEntityService _templateService;
    private readonly IUnitOfWork _unitOfWork;

    public TemplateCrudFactory(ITemplateEntityService templateService, IUnitOfWork unitOfWork)
    {
        _templateService = templateService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CertificateTemplate>> GetTemplatesAsync()
        => await _templateService.GetActiveTemplatesAsync();

    public async Task<CertificateTemplate?> GetTemplateAsync(int id)
        => await _templateService.GetByIdAsync(id);

    public async Task<CertificateTemplate> CreateTemplateAsync(CertificateTemplate template)
    {
        _templateService.Add(template);
        await _unitOfWork.SaveChangesAsync();
        return template;
    }

    public async Task UpdateTemplateAsync(CertificateTemplate template)
    {
        template.UpdatedAt = DateTime.UtcNow;
        _templateService.Update(template);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteTemplateAsync(int id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null) return false;
        template.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
