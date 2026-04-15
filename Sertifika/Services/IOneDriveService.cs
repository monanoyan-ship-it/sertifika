namespace Sertifika.Services;

public interface IOneDriveService
{
    Task<(bool Success, string? Error)> TestConnectionAsync();
    Task<OneDriveTestResult> TestAccountAsync(int accountId);
    Task EnsureFolderAsync(int accountId, string folderPath);
}

public class OneDriveTestResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public long? QuotaTotalBytes { get; set; }
    public long? QuotaUsedBytes { get; set; }
    public string? DriveType { get; set; }
    public string? OwnerName { get; set; }
}

internal class OneDriveUploadResult
{
    public int Total { get; set; }
    public int Uploaded { get; set; }
    public int Failed { get; set; }
    public string FolderPath { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FileItemIds { get; set; } = new();
}
