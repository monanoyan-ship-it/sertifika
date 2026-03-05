using Sertifika.Entities;

namespace Sertifika.Factories.Signatures;

public interface ISignatureCrudFactory
{
    Task<IEnumerable<Signature>> GetSignaturesAsync();
    Task<Signature?> GetSignatureAsync(int id);
    Task<Signature> CreateSignatureAsync(string name, string title, string imageUrl);
    Task UpdateSignatureAsync(Signature signature);
    Task<bool> DeleteSignatureAsync(int id);
}
