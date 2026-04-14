using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Companies;

public class CompanyCrudFactory : ICompanyCrudFactory
{
    private readonly ICompanyEntityService _companyService;
    private readonly IContactEntityService _contactService;
    private readonly ITrainingEntityService _trainingService;
    private readonly IParticipantEntityService _participantService;
    private readonly IUnitOfWork _unitOfWork;

    public CompanyCrudFactory(
        ICompanyEntityService companyService,
        IContactEntityService contactService,
        ITrainingEntityService trainingService,
        IParticipantEntityService participantService,
        IUnitOfWork unitOfWork)
    {
        _companyService = companyService;
        _contactService = contactService;
        _trainingService = trainingService;
        _participantService = participantService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Company>> GetCompaniesAsync()
        => await _companyService.GetActiveCompaniesAsync();

    public async Task<Company?> GetCompanyAsync(int id)
        => await _companyService.GetByIdAsync(id);

    public async Task<CompanyDetailDto?> GetCompanyDetailAsync(int id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null || !company.IsActive) return null;

        var contacts = await _contactService.GetByCompanyIdAsync(id);
        var trainings = (await _trainingService.GetByCompanyIdAsync(id)).ToList();

        var trainingSummaries = trainings.Select(t => new TrainingSummaryDto
        {
            Id = t.Id,
            Name = t.Name,
            TrainingDate = t.TrainingDate,
            EndDate = t.EndDate,
            InstructorName = t.InstructorName,
            ParticipantCount = t.Participants?.Count(p => p.IsActive) ?? 0,
            Status = (int)t.Status
        });

        var certificates = trainings
            .SelectMany(t => (t.Participants ?? Enumerable.Empty<Participant>())
                .Where(p => p.IsActive && !string.IsNullOrEmpty(p.CertificateNumber))
                .Select(p => new CertificateSummaryDto
                {
                    ParticipantId = p.Id,
                    FullName = $"{p.FirstName} {p.LastName}",
                    Email = p.Email,
                    CertificateNumber = p.CertificateNumber,
                    TrainingName = t.Name,
                    TrainingDate = t.TrainingDate,
                    HasPdf = !string.IsNullOrEmpty(p.CertificateNumber)
                }));

        return new CompanyDetailDto
        {
            Company = company,
            Contacts = contacts,
            Trainings = trainingSummaries,
            Certificates = certificates
        };
    }

    public async Task<IEnumerable<Contact>> GetContactsAsync(int companyId)
        => await _contactService.GetByCompanyIdAsync(companyId);

    public async Task<Contact> CreateContactAsync(int companyId, Contact contact)
    {
        contact.CompanyId = companyId;
        _contactService.Add(contact);
        await _unitOfWork.SaveChangesAsync();
        return contact;
    }

    public async Task UpdateContactAsync(int companyId, Contact contact)
    {
        var existing = await _contactService.GetByIdAsync(contact.Id);
        if (existing == null || existing.CompanyId != companyId) return;

        existing.FirstName = contact.FirstName;
        existing.LastName = contact.LastName;
        existing.Email = contact.Email;
        existing.Phone = contact.Phone;
        existing.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteContactAsync(int companyId, int contactId)
    {
        var contact = await _contactService.GetByIdAsync(contactId);
        if (contact == null || contact.CompanyId != companyId) return false;
        contact.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Company> CreateCompanyAsync(Company company)
    {
        _companyService.Add(company);
        await _unitOfWork.SaveChangesAsync();
        return company;
    }

    public async Task UpdateCompanyAsync(Company company)
    {
        company.UpdatedAt = DateTime.UtcNow;
        _companyService.Update(company);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteCompanyAsync(int id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company == null) return false;
        company.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
