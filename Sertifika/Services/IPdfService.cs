namespace Sertifika.Services;

public interface IPdfService
{
    Task<byte[]> GenerateSingleAsync(PdfGenerateRequest request);
    Task<List<PdfBatchResult>> GenerateBatchAsync(PdfBatchRequest request);
    Task<byte[]> PreviewAsync(PdfGenerateRequest request);
}

public class PdfFieldLayout
{
    public string Type { get; set; } = "static";
    public string? Key { get; set; }
    public string? Text { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string FontFamily { get; set; } = "Helvetica";
    public double FontSize { get; set; } = 14;
    public string FontColor { get; set; } = "#000000";
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public string Alignment { get; set; } = "center";
}

public class PdfSignatureInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? ImageUrl { get; set; }
}

public class PdfGenerateRequest
{
    public string Orientation { get; set; } = "landscape";
    public string? BackgroundImagePath { get; set; }
    public List<PdfFieldLayout> Layout { get; set; } = new();
    public List<PdfSignatureInfo> Signatures { get; set; } = new();
    public Dictionary<string, string> DynamicValues { get; set; } = new();
    public string? OutputFilename { get; set; }
}

public class PdfBatchRequest
{
    public string Orientation { get; set; } = "landscape";
    public string? BackgroundImagePath { get; set; }
    public List<PdfFieldLayout> Layout { get; set; } = new();
    public List<PdfSignatureInfo> Signatures { get; set; } = new();
    public List<Dictionary<string, string>> Participants { get; set; } = new();
    public string OutputDir { get; set; } = string.Empty;
}

public class PdfBatchResult
{
    public string Participant { get; set; } = string.Empty;
    public string? File { get; set; }
    public string? Error { get; set; }
    public bool Success { get; set; }
}
