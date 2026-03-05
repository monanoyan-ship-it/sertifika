using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface IHolderEntityService
{
    Task<IEnumerable<Holder>> GetActiveHoldersAsync();
    Task<Holder?> GetByIdAsync(int id);
    Task<Holder?> GetByEmailAsync(string email);
    void Add(Holder holder);
    void Update(Holder holder);
}
