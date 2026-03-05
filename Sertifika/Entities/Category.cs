namespace Sertifika.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}
