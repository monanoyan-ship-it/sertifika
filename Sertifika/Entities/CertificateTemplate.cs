namespace Sertifika.Entities;

public class CertificateTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PageOrientation Orientation { get; set; } = PageOrientation.Landscape;
    public string? BackgroundImageUrl { get; set; }
    public string LayoutJson { get; set; } = "[]";

    public ICollection<TemplateSignature> TemplateSignatures { get; set; } = new List<TemplateSignature>();
}

public class TemplateSignature : BaseEntity
{
    public int TemplateId { get; set; }
    public CertificateTemplate Template { get; set; } = null!;

    public int SignatureId { get; set; }
    public Signature Signature { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public string? InstructorName { get; set; }
    public string? InstructorTitle { get; set; }

    public bool ShowName { get; set; } = true;
    public bool ShowTitle { get; set; } = true;

    // Positions as percentages (0-100) of page dimensions
    public double ImageX { get; set; }
    public double ImageY { get; set; }
    public double ImageWidth { get; set; } = 12;
    public double ImageHeight { get; set; } = 8;
    public int ImageRotation { get; set; } // 0, 90, 180, 270
    public double NameX { get; set; }
    public double NameY { get; set; }
    public double TitleX { get; set; }
    public double TitleY { get; set; }
    public int NameFontSize { get; set; } = 8;
    public int TitleFontSize { get; set; } = 7;
}

public enum PageOrientation
{
    Landscape = 0,
    Portrait = 1
}
