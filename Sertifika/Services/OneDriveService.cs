using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Identity;

namespace Sertifika.Services;

public class OneDriveService : IOneDriveService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public OneDriveService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    public async Task<OneDriveUploadResult> ArchiveTrainingCertificatesAsync(
        int trainingId, string companyName, string trainingName, DateTime trainingDate)
    {
        var config = _configuration.GetSection("OneDrive");
        var tenantId = config["TenantId"];
        var clientId = config["ClientId"];
        var clientSecret = config["ClientSecret"];
        var driveUserId = config["DriveUserId"];

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            throw new InvalidOperationException("OneDrive configuration is not set");

        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
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
                var uploadUrl = $"https://graph.microsoft.com/v1.0/users/{driveUserId}/drive/root:/{folderPath}/{fileName}:/content";

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
