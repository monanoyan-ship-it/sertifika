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

    public async Task<CertificateTemplate?> GetTemplateWithSignaturesAsync(int id)
        => await _templateService.GetByIdWithSignaturesAsync(id);

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

    public async Task UpdateTemplateSignaturesAsync(int templateId, List<TemplateSignatureInput> signatures)
    {
        var template = await _templateService.GetByIdWithSignaturesAsync(templateId);
        if (template == null) return;

        _templateService.RemoveTemplateSignatures(template.TemplateSignatures);

        for (int i = 0; i < signatures.Count; i++)
        {
            var input = signatures[i];
            _templateService.AddTemplateSignature(new TemplateSignature
            {
                TemplateId = templateId,
                SignatureId = input.SignatureId,
                DisplayOrder = i,
                InstructorName = input.InstructorName,
                InstructorTitle = input.InstructorTitle,
                ShowName = input.ShowName,
                ShowTitle = input.ShowTitle,
                ImageX = input.ImageX,
                ImageY = input.ImageY,
                ImageWidth = input.ImageWidth,
                ImageHeight = input.ImageHeight,
                ImageRotation = input.ImageRotation,
                NameX = input.NameX,
                NameY = input.NameY,
                TitleX = input.TitleX,
                TitleY = input.TitleY
            });
        }

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
