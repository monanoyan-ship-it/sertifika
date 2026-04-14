namespace Sertifika.Entities;

public class TemplateField
{
    public string FieldType { get; set; } = string.Empty; // static, dynamic, qrcode
    public string? DynamicKey { get; set; }
    public string? StaticText { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    // Typography
    public string FontFamily { get; set; } = "Arial";
    public double FontSize { get; set; } = 14;
    public string FontColor { get; set; } = "#000000";
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
    public string TextAlign { get; set; } = "center";
    public double? LetterSpacing { get; set; }
    public double? LineHeight { get; set; }

    // Layer / z-order
    public int DisplayOrder { get; set; }
}

public static class DynamicFieldKeys
{
    public const string HolderName = "HolderName";
    public const string TrainingName = "TrainingName";
    public const string ProgramName = "ProgramName";
    public const string TrainingDate = "TrainingDate";
    public const string CompanyName = "CompanyName";
    public const string CertificateNo = "CertificateNo";
    public const string InstructorName = "InstructorName";
    public const string QrCode = "QrCode";

    public static readonly IReadOnlyList<FieldDescriptor> All = new List<FieldDescriptor>
    {
        new() { Key = HolderName, Label = "Katilimci Ad Soyad", Sample = "Ahmet Yilmaz" },
        new() { Key = TrainingName, Label = "Egitim Adi", Sample = "Is Guvenligi Egitimi" },
        new() { Key = ProgramName, Label = "Program Adi", Sample = "Isci Saglik ve Guvenligi Programi" },
        new() { Key = TrainingDate, Label = "Tarih", Sample = "10-11.04.2026" },
        new() { Key = CompanyName, Label = "Firma Adi", Sample = "ABC Ltd. Sti." },
        new() { Key = CertificateNo, Label = "Sertifika No", Sample = "CERT-20260410-0001-0001" },
        new() { Key = InstructorName, Label = "Egitmen Adi", Sample = "Dr. Mehmet Kaya" },
        new() { Key = QrCode, Label = "QR Kod", Sample = "" }
    };
}

public class FieldDescriptor
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Sample { get; set; } = string.Empty;
}
