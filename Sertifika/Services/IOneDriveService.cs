namespace Sertifika.Services;

public interface IOneDriveService
{
    Task<OneDriveUploadResult> ArchiveTrainingCertificatesAsync(int trainingId, string companyName, string trainingName, DateTime trainingDate);
    Task<byte[]?> DownloadFileAsync(string fileId);
    Task<string?> GetDownloadUrlAsync(string fileId);
    Task<(bool Success, string? Error)> TestConnectionAsync();
}

public class OneDriveUploadResult
{
    public int Total { get; set; }
    public int Uploaded { get; set; }
    public int Failed { get; set; }
    public string FolderPath { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> FileItemIds { get; set; } = new();
}
