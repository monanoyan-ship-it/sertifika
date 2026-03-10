namespace Sertifika.Services;

public interface IEmailService
{
    Task SendCertificateEmailAsync(string toEmail, string recipientName, string trainingName, string pdfFilePath);
    Task SendCertificateEmailAsync(string toEmail, string recipientName, string trainingName, string pdfFilePath, string? companyName, string? certificateNo);
    Task<EmailBatchResult> SendBatchAsync(List<EmailRecipient> recipients, string trainingName, string? companyName = null);
}

public class EmailRecipient
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PdfFilePath { get; set; } = string.Empty;
    public string? CertificateNo { get; set; }
}

public class EmailBatchResult
{
    public int Total { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
