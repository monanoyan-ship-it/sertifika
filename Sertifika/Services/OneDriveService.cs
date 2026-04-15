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
    private readonly EncryptionService _crypto;

    public OneDriveService(
        IOneDriveAccountEntityService accountService,
        IWebHostEnvironment env,
        IConfiguration config,
        EncryptionService crypto)
    {
        _accountService = accountService;
        _env = env;
        _config = config;
        _crypto = crypto;
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
        var basePath = string.IsNullOrWhiteSpace(account.BasePath) ? "Sertifikalar" : account.BasePath.Trim().Trim('/');
        var folderPath = $"{basePath}/{safeName(companyName)}/{year}/{safeName(trainingName)}";

        await EnsureFolderChainAsync(graphClient, driveId, folderPath);

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
        var r = await TestAccountInternalAsync(account);
        return (r.Success, r.Error);
    }

    public async Task<OneDriveTestResult> TestAccountAsync(int accountId)
    {
        var account = await _accountService.GetByIdAsync(accountId);
        if (account == null || !account.IsActive)
            return new OneDriveTestResult { Success = false, Error = "Hesap bulunamadi" };
        return await TestAccountInternalAsync(account);
    }

    public async Task EnsureFolderAsync(int accountId, string folderPath)
    {
        var account = await _accountService.GetByIdAsync(accountId)
            ?? throw new InvalidOperationException("Hesap bulunamadi");
        var graphClient = CreateGraphClient(account);
        var driveId = GetDriveId(account);
        await EnsureFolderChainAsync(graphClient, driveId, folderPath);
    }

    private async Task<OneDriveTestResult> TestAccountInternalAsync(Entities.OneDriveAccount account)
    {
        try
        {
            var graphClient = CreateGraphClient(account);
            var driveId = GetDriveId(account);
            var drive = await graphClient.Drives[driveId].GetAsync(r =>
                r.QueryParameters.Select = new[] { "id", "driveType", "owner", "quota" });

            if (drive == null)
                return new OneDriveTestResult { Success = false, Error = "Drive bilgisi alinamadi" };

            // BasePath varsa mevcut olmasini garanti et (yoksa olustur).
            if (!string.IsNullOrWhiteSpace(account.BasePath))
                await EnsureFolderChainAsync(graphClient, driveId, account.BasePath.Trim().Trim('/'));

            return new OneDriveTestResult
            {
                Success = true,
                QuotaTotalBytes = drive.Quota?.Total,
                QuotaUsedBytes = drive.Quota?.Used,
                DriveType = drive.DriveType,
                OwnerName = drive.Owner?.User?.DisplayName
            };
        }
        catch (Exception ex)
        {
            return new OneDriveTestResult { Success = false, Error = ex.InnerException?.Message ?? ex.Message };
        }
    }

    /// <summary>
    /// Path uzerindeki her segmenti sirasiyla kontrol eder, yoksa olusturur.
    /// folderPath: "Sertifikalar/ACME/2026" gibi basta / yok.
    /// </summary>
    private async Task EnsureFolderChainAsync(GraphServiceClient client, string driveId, string folderPath)
    {
        var segments = folderPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return;

        var parentPath = "";
        foreach (var segment in segments)
        {
            var currentPath = string.IsNullOrEmpty(parentPath) ? segment : $"{parentPath}/{segment}";
            try
            {
                await client.Drives[driveId].Root.ItemWithPath(currentPath).GetAsync();
            }
            catch
            {
                // Yoksa parent'ta children endpoint'ine POST ile olustur.
                var newItem = new DriveItem
                {
                    Name = segment,
                    Folder = new Folder(),
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["@microsoft.graph.conflictBehavior"] = "rename"
                    }
                };
                if (string.IsNullOrEmpty(parentPath))
                    await client.Drives[driveId].Items["root"].Children.PostAsync(newItem);
                else
                    await client.Drives[driveId].Root.ItemWithPath(parentPath).Children.PostAsync(newItem);
            }
            parentPath = currentPath;
        }
    }

    private GraphServiceClient CreateGraphClient(Entities.OneDriveAccount account)
    {
        var refreshToken = _crypto.Decrypt(account.RefreshToken);
        var clientSecret = _crypto.Decrypt(account.ClientSecret);

        if (!string.IsNullOrEmpty(refreshToken))
        {
            var clientId = !string.IsNullOrEmpty(account.ClientId)
                ? account.ClientId
                : _config["OneDrive:ClientId"] ?? "";
            var secret = !string.IsNullOrEmpty(clientSecret)
                ? clientSecret
                : _config["OneDrive:ClientSecret"] ?? "";

            var tokenCredential = new OneDriveRefreshTokenCredential(
                clientId, secret, refreshToken, account.TenantId);
            return new GraphServiceClient(tokenCredential);
        }

        var credential = new ClientSecretCredential(account.TenantId, account.ClientId, clientSecret);
        return new GraphServiceClient(credential);
    }

    private string GetDriveId(Entities.OneDriveAccount account)
    {
        if (!string.IsNullOrEmpty(account.DriveId))
            return account.DriveId;
        throw new InvalidOperationException("OneDrive DriveId yapilandirilmamis. Hesabi yeniden baglayip drive secin.");
    }
}
