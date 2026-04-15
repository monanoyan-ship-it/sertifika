namespace Sertifika.Entities;

public class SmtpAccount : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Classic SMTP connection
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public bool UseSsl { get; set; } = true;

    // OAuth 2.0 / Microsoft Graph (M365)
    // UseOAuth=true -> SMTP XOAUTH2 ile gonderim (Host/Port hala kullanilir)
    // UseGraphApi=true -> SMTP atlanir, Microsoft Graph /sendMail endpointine gider (Host/Port kullanilmaz)
    public bool UseOAuth { get; set; }
    public bool UseGraphApi { get; set; }
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    // Profile state
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastTestedAt { get; set; }
    public string? LastTestStatus { get; set; }
    public string? LastTestError { get; set; }
}
