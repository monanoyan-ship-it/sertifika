namespace Sertifika.Entities;

public class OneDriveAccount : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string DriveUserId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string DriveId { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
