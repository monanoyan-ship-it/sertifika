using Sertifika.Entities;

namespace Sertifika.Factories.Holders;

public interface IHolderCrudFactory
{
    Task<IEnumerable<Holder>> GetHoldersAsync();
    Task<Holder?> GetHolderAsync(int id);
    Task<Holder> CreateHolderAsync(Holder holder);
    Task UpdateHolderAsync(Holder holder);
    Task<bool> DeleteHolderAsync(int id);
}
