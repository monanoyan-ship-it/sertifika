using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface ISignatureEntityService
{
    Task<IEnumerable<Signature>> GetActiveSignaturesAsync();
    Task<Signature?> GetByIdAsync(int id);
    void Add(Signature signature);
    void Update(Signature signature);
}
