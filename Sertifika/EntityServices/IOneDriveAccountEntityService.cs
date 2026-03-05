using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface IOneDriveAccountEntityService
{
    Task<IEnumerable<OneDriveAccount>> GetActiveAccountsAsync();
    Task<OneDriveAccount?> GetByIdAsync(int id);
    Task<OneDriveAccount?> GetDefaultAccountAsync();
    void Add(OneDriveAccount account);
    void Update(OneDriveAccount account);
}
