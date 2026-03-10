using System.Net;
using System.Net.Mail;
using Sertifika.EntityServices;

namespace Sertifika.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ISmtpAccountEntityService _smtpAccountService;
    private readonly IEmailTemplateEntityService _emailTemplateService;

    public EmailService(IConfiguration configuration, ISmtpAccountEntityService smtpAccountService, IEmailTemplateEntityService emailTemplateService)
    {
        _configuration = configuration;
        _smtpAccountService = smtpAccountService;
        _emailTemplateService = emailTemplateService;
    }

    public Task SendCertificateEmailAsync(string toEmail, string recipientName, string trainingName, string pdfFilePath)
        => SendCertificateEmailAsync(toEmail, recipientName, trainingName, pdfFilePath, null, null);

    public async Task SendCertificateEmailAsync(string toEmail, string recipientName, string trainingName, string pdfFilePath, string? companyName, string? certificateNo)
    {
        var smtp = await GetSmtpSettingsAsync();
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

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            Credentials = new NetworkCredential(smtp.Username, smtp.Password),
            EnableSsl = smtp.UseSsl
        };

        using var message = new MailMessage
        {
            From = new MailAddress(smtp.FromEmail, smtp.FromName ?? "Sertifika Sistemi"),
            Subject = subject,
            IsBodyHtml = true,
            Body = body
        };

        message.To.Add(new MailAddress(toEmail, recipientName));

        if (File.Exists(pdfFilePath))
        {
            message.Attachments.Add(new Attachment(pdfFilePath));
        }

        await client.SendMailAsync(message);
    }

    public async Task<EmailBatchResult> SendBatchAsync(List<EmailRecipient> recipients, string trainingName, string? companyName = null)
    {
        var result = new EmailBatchResult { Total = recipients.Count };

        foreach (var recipient in recipients)
        {
            try
            {
                await SendCertificateEmailAsync(recipient.Email, recipient.Name, trainingName, recipient.PdfFilePath, companyName, recipient.CertificateNo);
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

    private async Task<SmtpSettings> GetSmtpSettingsAsync()
    {
        var dbAccount = await _smtpAccountService.GetDefaultAccountAsync();
        if (dbAccount != null)
        {
            return new SmtpSettings
            {
                Host = dbAccount.Host,
                Port = dbAccount.Port,
                Username = dbAccount.Username,
                Password = dbAccount.Password,
                FromEmail = dbAccount.FromEmail,
                FromName = dbAccount.FromName,
                UseSsl = dbAccount.UseSsl
            };
        }

        // Fallback to appsettings.json
        var config = _configuration.GetSection("Smtp");
        return new SmtpSettings
        {
            Host = config["Host"] ?? "localhost",
            Port = int.Parse(config["Port"] ?? "587"),
            Username = config["Username"] ?? "",
            Password = config["Password"] ?? "",
            FromEmail = config["From"] ?? "noreply@example.com",
            FromName = config["FromName"],
            UseSsl = bool.Parse(config["EnableSsl"] ?? "true")
        };
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

    private class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string? FromName { get; set; }
        public bool UseSsl { get; set; }
    }
}
