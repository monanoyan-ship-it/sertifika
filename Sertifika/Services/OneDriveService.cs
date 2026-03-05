using System.Net.Http.Headers;
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

        var credential = new ClientSecretCredential(account.TenantId, account.ClientId, account.ClientSecret);
        var token = await credential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }));

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        // Folder path: Sertifikalar/{CompanyName}/{Year}/{TrainingName}
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
                var uploadUrl = $"https://graph.microsoft.com/v1.0/users/{account.DriveUserId}/drive/root:/{folderPath}/{fileName}:/content";

                using var fileStream = File.OpenRead(file);
                using var content = new StreamContent(fileStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                var response = await httpClient.PutAsync(uploadUrl, content);
                response.EnsureSuccessStatusCode();

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
