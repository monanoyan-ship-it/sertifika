namespace Sertifika.Entities;

public class CertificateTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PageOrientation Orientation { get; set; } = PageOrientation.Landscape;
    public string? BackgroundImageUrl { get; set; }
    public string LayoutJson { get; set; } = "[]";
}

public enum PageOrientation
{
    Landscape = 0,
    Portrait = 1
}
