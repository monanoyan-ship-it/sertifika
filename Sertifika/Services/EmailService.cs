using System.Net;
using System.Net.Mail;

namespace Sertifika.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendCertificateEmailAsync(string toEmail, string recipientName, string trainingName, string pdfFilePath)
    {
        var smtpConfig = _configuration.GetSection("Smtp");

        using var client = new SmtpClient(smtpConfig["Host"], int.Parse(smtpConfig["Port"]!))
        {
            Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"]),
            EnableSsl = bool.Parse(smtpConfig["EnableSsl"] ?? "true")
        };

        using var message = new MailMessage
        {
            From = new MailAddress(smtpConfig["From"]!, smtpConfig["FromName"] ?? "Sertifika Sistemi"),
            Subject = $"Sertifikaniz - {trainingName}",
            IsBodyHtml = true,
            Body = $"""
                <h2>Merhaba {recipientName},</h2>
                <p><strong>{trainingName}</strong> egitimi icin sertifikaniz ekte yer almaktadir.</p>
                <p>Sertifikanizi dogrulamak icin sertifika uzerindeki QR kodu kullanabilirsiniz.</p>
                <br>
                <p>Iyi gunler dileriz.</p>
                <p><em>Sertifika Yonetim Sistemi</em></p>
                """
        };

        message.To.Add(new MailAddress(toEmail, recipientName));

        if (File.Exists(pdfFilePath))
        {
            message.Attachments.Add(new Attachment(pdfFilePath));
        }

        await client.SendMailAsync(message);
    }

    public async Task<EmailBatchResult> SendBatchAsync(List<EmailRecipient> recipients, string trainingName)
    {
        var result = new EmailBatchResult { Total = recipients.Count };

        foreach (var recipient in recipients)
        {
            try
            {
                await SendCertificateEmailAsync(recipient.Email, recipient.Name, trainingName, recipient.PdfFilePath);
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
}
