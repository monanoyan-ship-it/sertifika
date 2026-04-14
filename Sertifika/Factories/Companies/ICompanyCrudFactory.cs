using Sertifika.Entities;

namespace Sertifika.Factories.Companies;

public interface ICompanyCrudFactory
{
    Task<IEnumerable<Company>> GetCompaniesAsync();
    Task<Company?> GetCompanyAsync(int id);
    Task<CompanyDetailDto?> GetCompanyDetailAsync(int id);
    Task<IEnumerable<Contact>> GetContactsAsync(int companyId);
    Task<Contact> CreateContactAsync(int companyId, Contact contact);
    Task UpdateContactAsync(int companyId, Contact contact);
    Task<bool> DeleteContactAsync(int companyId, int contactId);
    Task<Company> CreateCompanyAsync(Company company);
    Task UpdateCompanyAsync(Company company);
    Task<bool> DeleteCompanyAsync(int id);
}

public class CompanyDetailDto
{
    public Company Company { get; set; } = null!;
    public IEnumerable<Contact> Contacts { get; set; } = Array.Empty<Contact>();
    public IEnumerable<TrainingSummaryDto> Trainings { get; set; } = Array.Empty<TrainingSummaryDto>();
    public IEnumerable<CertificateSummaryDto> Certificates { get; set; } = Array.Empty<CertificateSummaryDto>();
}

public class TrainingSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime TrainingDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? InstructorName { get; set; }
    public int ParticipantCount { get; set; }
    public int Status { get; set; }
}

public class CertificateSummaryDto
{
    public int ParticipantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? CertificateNumber { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public DateTime TrainingDate { get; set; }
    public bool HasPdf { get; set; }
}
