using Sertifika.Entities;

namespace Sertifika.Factories.Templates;

public interface ITemplateCrudFactory
{
    Task<IEnumerable<CertificateTemplate>> GetTemplatesAsync();
    Task<CertificateTemplate?> GetTemplateAsync(int id);
    Task<CertificateTemplate?> GetTemplateWithSignaturesAsync(int id);
    Task<CertificateTemplate> CreateTemplateAsync(CertificateTemplate template);
    Task UpdateTemplateAsync(CertificateTemplate template);
    Task UpdateTemplateSignaturesAsync(int templateId, List<TemplateSignatureInput> signatures);
    Task<bool> DeleteTemplateAsync(int id);
}

public class TemplateSignatureInput
{
    public int SignatureId { get; set; }
    public string? InstructorName { get; set; }
    public string? InstructorTitle { get; set; }
    public bool ShowName { get; set; } = true;
    public bool ShowTitle { get; set; } = true;
    public double ImageX { get; set; }
    public double ImageY { get; set; }
    public double ImageWidth { get; set; } = 12;
    public double ImageHeight { get; set; } = 8;
    public int ImageRotation { get; set; }
    public double NameX { get; set; }
    public double NameY { get; set; }
    public double TitleX { get; set; }
    public double TitleY { get; set; }
}
