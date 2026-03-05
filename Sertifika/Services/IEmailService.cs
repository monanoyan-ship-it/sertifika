namespace Sertifika.Services;

public interface IEmailService
{
    Task SendCertificateEmailAsync(string toEmail, string recipientName, string trainingName, string pdfFilePath);
    Task<EmailBatchResult> SendBatchAsync(List<EmailRecipient> recipients, string trainingName);
}

public class EmailRecipient
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PdfFilePath { get; set; } = string.Empty;
}

public class EmailBatchResult
{
    public int Total { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
