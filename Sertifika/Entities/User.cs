using System.Text.Json.Serialization;

namespace Sertifika.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Admin = 0,
    CertificateCreator = 1,
    Viewer = 2
}
