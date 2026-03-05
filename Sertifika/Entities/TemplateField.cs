namespace Sertifika.Entities;

public class TemplateField
{
    public string FieldType { get; set; } = string.Empty; // text, dynamic, image
    public string? DynamicKey { get; set; } // HolderName, TrainingName, TrainingDate, CompanyName, CertificateNo, QrCode
    public string? StaticText { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string FontFamily { get; set; } = "Arial";
    public double FontSize { get; set; } = 14;
    public string FontColor { get; set; } = "#000000";
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public string TextAlign { get; set; } = "center"; // left, center, right
}

public static class DynamicFieldKeys
{
    public const string HolderName = "HolderName";
    public const string TrainingName = "TrainingName";
    public const string TrainingDate = "TrainingDate";
    public const string CompanyName = "CompanyName";
    public const string CertificateNo = "CertificateNo";
    public const string QrCode = "QrCode";
}
