namespace Sertifika.Entities;

public class OneDriveAccount : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;

    // Hassas alanlar: AES sifreli (EncryptionService ile). DB'de "enc:" prefix'li base64.
    public string ClientSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;

    public string DriveUserId { get; set; } = string.Empty;
    public string DriveId { get; set; } = string.Empty;

    // Yuklemelerde kullanilacak kok klasor (UI'den editlenebilir). Bos ise "Sertifikalar".
    public string BasePath { get; set; } = "Sertifikalar";

    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; } = true;

    public DateTime? LastTestedAt { get; set; }
    public string? LastTestStatus { get; set; }
    public string? LastTestError { get; set; }

    // Quota (en son getirilen degerler; her test-connection/quota-refresh'te guncellenir)
    public long? QuotaTotalBytes { get; set; }
    public long? QuotaUsedBytes { get; set; }
    public DateTime? QuotaCheckedAt { get; set; }
}
