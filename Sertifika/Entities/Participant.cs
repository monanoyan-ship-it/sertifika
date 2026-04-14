namespace Sertifika.Entities;

public enum CertificateStorageType
{
    Local = 0,
    OneDrive = 1
}

public class Participant : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? CompanyName { get; set; }
    public string? CertificateNumber { get; set; }
    public string? CertificatePdfUrl { get; set; }
    public CertificateStorageType StorageType { get; set; } = CertificateStorageType.Local;
    public string? CloudFileId { get; set; }

    public int TrainingId { get; set; }
    public Training Training { get; set; } = null!;

    public int? ContactId { get; set; }
    public Contact? Contact { get; set; }
}
