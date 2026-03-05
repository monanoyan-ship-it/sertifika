using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface IUserEntityService
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    void Add(User user);
}
