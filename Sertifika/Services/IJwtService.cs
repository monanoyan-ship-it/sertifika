using Sertifika.Entities;

namespace Sertifika.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
