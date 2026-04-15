using Sertifika.Entities;
using Sertifika.EntityServices;

namespace Sertifika.Services;

public class EmailService : IEmailService
{
    private readonly ISmtpAccountEntityService _smtpAccountService;
    private readonly IEmailTemplateEntityService _emailTemplateService;
    private readonly SmtpMailDispatcher _dispatcher;

    public EmailService(
        ISmtpAccountEntityService smtpAccountService,
        IEmailTemplateEntityService emailTemplateService,
        SmtpMailDispatcher dispatcher)
    {
        _smtpAccountService = smtpAccountService;
        _emailTemplateService = emailTemplateService;
        _dispatcher = dispatcher;
    }

    public async Task SendCertificateEmailAsync(string toEmail, string recipientName, string trainingName,
        byte[] pdfBytes, string pdfFilename, string? companyName = null, string? certificateNo = null)
    {
        var account = await _smtpAccountService.GetDefaultAccountAsync()
            ?? throw new InvalidOperationException("Varsayilan SMTP hesabi yok. Ayarlar > SMTP uzerinden bir hesap ekleyin ve varsayilan isaretleyin.");

        if (!account.IsEnabled)
            throw new InvalidOperationException($"'{account.Name}' SMTP hesabi devre disi.");

        var template = await _emailTemplateService.GetDefaultTemplateAsync();

        string subject;
        string body;
        if (template != null)
        {
            subject = ReplacePlaceholders(template.Subject, recipientName, trainingName, companyName, certificateNo);
            body = ReplacePlaceholders(template.Body, recipientName, trainingName, companyName, certificateNo);
        }
        else
        {
            subject = $"Sertifikaniz - {trainingName}";
            body = $"""
                <h2>Merhaba {recipientName},</h2>
                <p><strong>{trainingName}</strong> egitimi icin sertifikaniz ekte yer almaktadir.</p>
                <p>Sertifikanizi dogrulamak icin sertifika uzerindeki QR kodu kullanabilirsiniz.</p>
                <br>
                <p>Iyi gunler dileriz.</p>
                <p><em>Sertifika Yonetim Sistemi</em></p>
                """;
        }

        var message = new EmailMessage
        {
            ToEmail = toEmail,
            ToName = recipientName,
            Subject = subject,
            HtmlBody = body,
            Attachments = pdfBytes.Length > 0
                ? new List<EmailAttachment>
                {
                    new() { Filename = pdfFilename, ContentType = "application/pdf", Bytes = pdfBytes }
                }
                : new List<EmailAttachment>()
        };

        await _dispatcher.SendAsync(account, message);
    }

    public async Task<EmailBatchResult> SendBatchAsync(List<EmailRecipient> recipients, string trainingName, string? companyName = null)
    {
        var result = new EmailBatchResult { Total = recipients.Count };

        foreach (var recipient in recipients)
        {
            try
            {
                await SendCertificateEmailAsync(recipient.Email, recipient.Name, trainingName,
                    recipient.PdfBytes, recipient.PdfFilename, companyName, recipient.CertificateNo);
                result.Sent++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"{recipient.Name} ({recipient.Email}): {ex.Message}");
            }
        }

        return result;
    }

    private static string ReplacePlaceholders(string text, string recipientName, string trainingName, string? companyName, string? certificateNo)
    {
        return text
            .Replace("{RecipientName}", recipientName)
            .Replace("{TrainingName}", trainingName)
            .Replace("{TrainingDate}", DateTime.Now.ToString("dd.MM.yyyy"))
            .Replace("{CompanyName}", companyName ?? "")
            .Replace("{CertificateNo}", certificateNo ?? "");
    }
}
