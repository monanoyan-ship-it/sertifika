using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Identity;
using Sertifika.EntityServices;

namespace Sertifika.Services;

public class OneDriveService : IOneDriveService
{
    private readonly IOneDriveAccountEntityService _accountService;
    private readonly IWebHostEnvironment _env;

    public OneDriveService(IOneDriveAccountEntityService accountService, IWebHostEnvironment env)
    {
        _accountService = accountService;
        _env = env;
    }

    public async Task<OneDriveUploadResult> ArchiveTrainingCertificatesAsync(
        int trainingId, string companyName, string trainingName, DateTime trainingDate)
    {
        var account = await _accountService.GetDefaultAccountAsync();
        if (account == null)
            throw new InvalidOperationException("Varsayilan OneDrive hesabi bulunamadi. Ayarlar sayfasindan bir hesap ekleyip varsayilan olarak isaretleyin.");

        var (httpClient, driveUserId) = await CreateGraphClient(account);
        using (httpClient)
        {
            var year = trainingDate.Year.ToString();
            var safeName = (string name) => name.Replace("/", "_").Replace("\\", "_").Replace(":", "_");
            var folderPath = $"Sertifikalar/{safeName(companyName)}/{year}/{safeName(trainingName)}";

            var certDir = Path.Combine(
                _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
                "uploads", "certificates", $"training_{trainingId}");

            var result = new OneDriveUploadResult { FolderPath = folderPath };

            if (!Directory.Exists(certDir))
            {
                result.Errors.Add("Certificate directory not found");
                return result;
            }

            var pdfFiles = Directory.GetFiles(certDir, "*.pdf");
            result.Total = pdfFiles.Length;

            foreach (var file in pdfFiles)
            {
                try
                {
                    var fileName = Path.GetFileName(file);
                    var uploadUrl = $"https://graph.microsoft.com/v1.0/users/{driveUserId}/drive/root:/{folderPath}/{fileName}:/content";

                    using var fileStream = File.OpenRead(file);
                    using var content = new StreamContent(fileStream);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                    var response = await httpClient.PutAsync(uploadUrl, content);
                    response.EnsureSuccessStatusCode();

                    // Parse response to get OneDrive item ID
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(responseJson);
                    if (doc.RootElement.TryGetProperty("id", out var idProp))
                    {
                        result.FileItemIds[fileName] = idProp.GetString()!;
                    }

                    result.Uploaded++;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                }
            }

            return result;
        }
    }

    public async Task<byte[]?> DownloadFileAsync(string fileId)
    {
        var account = await _accountService.GetDefaultAccountAsync();
        if (account == null) return null;

        var (httpClient, driveUserId) = await CreateGraphClient(account);
        using (httpClient)
        {
            var url = $"https://graph.microsoft.com/v1.0/users/{driveUserId}/drive/items/{fileId}/content";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync();
        }
    }

    private async Task<(HttpClient client, string driveUserId)> CreateGraphClient(Entities.OneDriveAccount account)
    {
        var credential = new ClientSecretCredential(account.TenantId, account.ClientId, account.ClientSecret);
        var token = await credential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }));

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return (httpClient, account.DriveUserId);
    }
}
