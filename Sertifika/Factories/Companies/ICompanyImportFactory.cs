namespace Sertifika.Factories.Companies;

public interface ICompanyImportFactory
{
    Task<ImportPreviewResult> PreviewAsync(Stream csvStream);
    Task<ImportConfirmResult> ConfirmAsync(Stream csvStream, int defaultTemplateId);
}

public class ImportPreviewResult
{
    public List<string> NewCompanies { get; set; } = new();
    public List<PreviewTraining> NewTrainings { get; set; } = new();
    public List<PreviewContact> NewContacts { get; set; } = new();
    public int NewParticipations { get; set; }
    public int ExistingParticipations { get; set; }
    public int TotalRows { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class PreviewTraining
{
    public string CompanyName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime TrainingDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? InstructorName { get; set; }
}

public class PreviewContact
{
    public string CompanyName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class ImportConfirmResult
{
    public int CompaniesCreated { get; set; }
    public int ContactsCreated { get; set; }
    public int TrainingsCreated { get; set; }
    public int ParticipantsCreated { get; set; }
    public int RowsSkipped { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class CsvRow
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string TrainingName { get; set; } = string.Empty;
    public string? InstructorName { get; set; }
    public DateTime TrainingDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int LineNumber { get; set; }
}
