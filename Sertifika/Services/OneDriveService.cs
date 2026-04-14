using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Sertifika.EntityServices;

namespace Sertifika.Services;

public class OneDriveService : IOneDriveService
{
    private readonly IOneDriveAccountEntityService _accountService;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public OneDriveService(IOneDriveAccountEntityService accountService, IWebHostEnvironment env, IConfiguration config)
    {
        _accountService = accountService;
        _env = env;
        _config = config;
    }

    private async Task<OneDriveUploadResult> ArchiveTrainingCertificatesAsync(
        int trainingId, string companyName, string trainingName, DateTime trainingDate)
    {
        var account = await _accountService.GetDefaultAccountAsync();
        if (account == null)
            throw new InvalidOperationException("Varsayilan OneDrive hesabi bulunamadi. Ayarlar sayfasindan bir hesap ekleyip varsayilan olarak isaretleyin.");

        var graphClient = CreateGraphClient(account);
        var driveId = GetDriveId(account);

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
                var targetPath = $"/{folderPath}/{fileName}";

                using var fileStream = File.OpenRead(file);

                if (fileStream.Length <= 4 * 1024 * 1024)
                {
                    var driveItem = await graphClient.Drives[driveId]
                        .Root
                        .ItemWithPath(targetPath)
                        .Content
                        .PutAsync(fileStream);

                    if (driveItem?.Id != null)
                        result.FileItemIds[fileName] = driveItem.Id;
                }
                else
                {
                    var uploadSession = await graphClient.Drives[driveId]
                        .Root
                        .ItemWithPath(targetPath)
                        .CreateUploadSession
                        .PostAsync(new Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession.CreateUploadSessionPostRequestBody
                        {
                            Item = new DriveItemUploadableProperties { Name = fileName }
                        });

                    if (uploadSession?.UploadUrl != null)
                    {
                        const int chunkSize = 5 * 1024 * 1024;
                        var fileUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, fileStream, chunkSize);
                        var uploadResult = await fileUploadTask.UploadAsync();

                        if (uploadResult.UploadSucceeded && uploadResult.ItemResponse is DriveItem item)
                            result.FileItemIds[fileName] = item.Id!;
                    }
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

    private async Task<byte[]?> DownloadFileAsync(string fileId)
    {
        var account = await _accountService.GetDefaultAccountAsync();
        if (account == null) return null;

        try
        {
            var graphClient = CreateGraphClient(account);
            var driveId = GetDriveId(account);

            var stream = await graphClient.Drives[driveId]
                .Items[fileId]
                .Content
                .GetAsync();

            if (stream == null) return null;

            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> GetDownloadUrlAsync(string fileId)
    {
        var account = await _accountService.GetDefaultAccountAsync();
        if (account == null) return null;

        try
        {
            var graphClient = CreateGraphClient(account);
            var driveId = GetDriveId(account);

            var item = await graphClient.Drives[driveId]
                .Items[fileId]
                .GetAsync(r => r.QueryParameters.Select = new[] { "id", "@microsoft.graph.downloadUrl" });

            if (item?.AdditionalData?.TryGetValue("@microsoft.graph.downloadUrl", out var url) == true)
                return url?.ToString();

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string? Error)> TestConnectionAsync()
    {
        var account = await _accountService.GetDefaultAccountAsync();
        if (account == null)
            return (false, "Varsayilan OneDrive hesabi bulunamadi");

        try
        {
            var graphClient = CreateGraphClient(account);
            var driveId = GetDriveId(account);
            var drive = await graphClient.Drives[driveId].GetAsync();
            return (drive != null, null);
        }
        catch (Exception ex)
        {
            return (false, $"OneDrive baglanti hatasi: {ex.Message}");
        }
    }

    private GraphServiceClient CreateGraphClient(Entities.OneDriveAccount account)
    {
        if (!string.IsNullOrEmpty(account.RefreshToken))
        {
            // OAuth flow: ClientId/Secret appsettings'ten, RefreshToken DB'den
            var clientId = !string.IsNullOrEmpty(account.ClientId)
                ? account.ClientId
                : _config["OneDrive:ClientId"] ?? "";
            var clientSecret = !string.IsNullOrEmpty(account.ClientSecret)
                ? account.ClientSecret
                : _config["OneDrive:ClientSecret"] ?? "";

            var tokenCredential = new OneDriveRefreshTokenCredential(
                clientId, clientSecret, account.RefreshToken, account.TenantId);
            return new GraphServiceClient(tokenCredential);
        }

        // Eski yontem: Application auth (ClientSecretCredential)
        var credential = new ClientSecretCredential(account.TenantId, account.ClientId, account.ClientSecret);
        return new GraphServiceClient(credential);
    }

    private string GetDriveId(Entities.OneDriveAccount account)
    {
        if (!string.IsNullOrEmpty(account.DriveId))
            return account.DriveId;

        throw new InvalidOperationException("OneDrive DriveId yapilandirilmamis. Hesabi yeniden baglayip drive secin.");
    }
}
