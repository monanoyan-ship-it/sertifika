namespace Sertifika.Entities;

public class Holder : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdentityNumber { get; set; }

    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}
