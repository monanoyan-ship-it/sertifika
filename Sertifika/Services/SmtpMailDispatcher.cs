using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Identity.Client;
using MimeKit;
using Sertifika.Entities;

namespace Sertifika.Services;

/// <summary>
/// Uc modu da yoneten tek giris noktasi:
///   1) Basic SMTP (Username + Password)
///   2) SMTP XOAUTH2 (Azure AD client credentials + SMTP.Send)
///   3) Microsoft Graph /users/{from}/sendMail (Mail.Send application permission)
/// </summary>
public class SmtpMailDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SmtpMailDispatcher(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task SendAsync(SmtpAccount account, EmailMessage message, CancellationToken ct = default)
    {
        if (account.UseGraphApi) return SendViaGraphAsync(account, message, ct);
        if (account.UseOAuth) return SendViaSmtpOAuthAsync(account, message, ct);
        return SendViaSmtpBasicAsync(account, message, ct);
    }

    // ─── 1) Basic SMTP ───

    private async Task SendViaSmtpBasicAsync(SmtpAccount account, EmailMessage message, CancellationToken ct)
    {
        using var client = new SmtpClient();
        var secure = account.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(account.Host, account.Port, secure, ct);
        if (!string.IsNullOrEmpty(account.Username))
            await client.AuthenticateAsync(account.Username, account.Password, ct);
        await client.SendAsync(BuildMimeMessage(account, message), ct);
        await client.DisconnectAsync(true, ct);
    }

    // ─── 2) SMTP XOAUTH2 (M365 / Google Workspace tenantli senaryolar) ───

    private async Task SendViaSmtpOAuthAsync(SmtpAccount account, EmailMessage message, CancellationToken ct)
    {
        EnsureOAuthCreds(account);

        var accessToken = await AcquireTokenAsync(
            account,
            new[] { "https://outlook.office365.com/.default" },
            ct);

        using var client = new SmtpClient();
        await client.ConnectAsync(account.Host, account.Port, SecureSocketOptions.StartTls, ct);

        var oauth2 = new SaslMechanismOAuth2(account.Username, accessToken);
        await client.AuthenticateAsync(oauth2, ct);

        await client.SendAsync(BuildMimeMessage(account, message), ct);
        await client.DisconnectAsync(true, ct);
    }

    // ─── 3) Microsoft Graph /sendMail ───

    private async Task SendViaGraphAsync(SmtpAccount account, EmailMessage message, CancellationToken ct)
    {
        EnsureOAuthCreds(account);

        var accessToken = await AcquireTokenAsync(
            account,
            new[] { "https://graph.microsoft.com/.default" },
            ct);

        var payload = new
        {
            message = new
            {
                subject = message.Subject,
                body = new { contentType = "HTML", content = message.HtmlBody },
                from = new { emailAddress = new { address = account.FromEmail, name = account.FromName ?? "" } },
                toRecipients = new[]
                {
                    new { emailAddress = new { address = message.ToEmail, name = message.ToName ?? "" } }
                },
                attachments = message.Attachments.Count == 0
                    ? null
                    : message.Attachments.Select(a => new
                    {
                        odataType = "#microsoft.graph.fileAttachment",
                        name = a.Filename,
                        contentType = a.ContentType,
                        contentBytes = Convert.ToBase64String(a.Bytes)
                    }).ToArray<object?>()
            },
            saveToSentItems = true
        };

        // JSON'i manuel olusturmamiz lazim: "@odata.type" anahtarina C# anonymous tipinde izin yok.
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        if (message.Attachments.Count > 0)
            json = json.Replace("\"odataType\":", "\"@odata.type\":");

        var client = _httpClientFactory.CreateClient("GraphMail");
        client.BaseAddress = new Uri("https://graph.microsoft.com/");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"v1.0/users/{Uri.EscapeDataString(account.FromEmail)}/sendMail")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Graph sendMail basarisiz ({(int)response.StatusCode}): {body}");
        }
    }

    // ─── Helpers ───

    private async Task<string> AcquireTokenAsync(SmtpAccount account, string[] scopes, CancellationToken ct)
    {
        var app = ConfidentialClientApplicationBuilder
            .Create(account.ClientId)
            .WithClientSecret(account.ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{account.TenantId}"))
            .Build();

        var result = await app.AcquireTokenForClient(scopes).ExecuteAsync(ct);
        return result.AccessToken;
    }

    private static void EnsureOAuthCreds(SmtpAccount account)
    {
        if (string.IsNullOrWhiteSpace(account.TenantId) ||
            string.IsNullOrWhiteSpace(account.ClientId) ||
            string.IsNullOrWhiteSpace(account.ClientSecret))
            throw new InvalidOperationException("OAuth/Graph icin TenantId, ClientId ve ClientSecret zorunlu.");
    }

    private static MimeMessage BuildMimeMessage(SmtpAccount account, EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(account.FromName ?? "", account.FromEmail));
        mime.To.Add(new MailboxAddress(message.ToName ?? "", message.ToEmail));
        mime.Subject = message.Subject;

        var builder = new BodyBuilder { HtmlBody = message.HtmlBody };
        foreach (var att in message.Attachments)
            builder.Attachments.Add(att.Filename, att.Bytes, ContentType.Parse(att.ContentType));
        mime.Body = builder.ToMessageBody();
        return mime;
    }
}

public class EmailMessage
{
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public List<EmailAttachment> Attachments { get; set; } = new();
}

public class EmailAttachment
{
    public string Filename { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
}
