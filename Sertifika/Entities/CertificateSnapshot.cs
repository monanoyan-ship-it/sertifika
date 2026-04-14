namespace Sertifika.Entities;

/// <summary>
/// Sertifika uretim anindaki tum veriyi saklar. PDF diske yazilmaz;
/// istendiginde bu snapshot'tan on-demand uretilir. Sablon/imza sonradan
/// degisse bile eski sertifika bu snapshot sayesinde yeniden olusturulabilir.
/// </summary>
public class CertificateSnapshot : BaseEntity
{
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;

    public string CertificateNumber { get; set; } = string.Empty;

    public string Orientation { get; set; } = "landscape";
    public string? BackgroundImagePath { get; set; }

    // Layout fields JSON (PdfFieldLayout list)
    public string LayoutJson { get; set; } = string.Empty;

    // Signatures JSON (PdfSignatureInfo list)
    public string SignaturesJson { get; set; } = string.Empty;

    // Dynamic values JSON: HolderName, TrainingName, TrainingDate,
    // CompanyName, CertificateNo, QrCode
    public string DynamicValuesJson { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
