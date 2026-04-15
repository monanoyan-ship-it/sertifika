using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface IUserEntityService
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    void Add(User user);
    void Update(User user);
    Task<bool> DeleteAsync(int id);
    Task<int> CountActiveAdminsAsync(int? excludeId = null);
}
