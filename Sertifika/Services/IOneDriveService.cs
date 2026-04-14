namespace Sertifika.Services;

public interface IOneDriveService
{
    Task<(bool Success, string? Error)> TestConnectionAsync();
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
