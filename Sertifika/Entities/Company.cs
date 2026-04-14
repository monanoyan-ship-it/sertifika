namespace Sertifika.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}

public class Contact : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
