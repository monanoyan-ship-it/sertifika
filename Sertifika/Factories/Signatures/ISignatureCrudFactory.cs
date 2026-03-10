using Sertifika.Entities;

namespace Sertifika.Factories.Signatures;

public interface ISignatureCrudFactory
{
    Task<IEnumerable<Signature>> GetSignaturesAsync();
    Task<Signature?> GetSignatureAsync(int id);
    Task<Signature> CreateSignatureAsync(string name, string title, string imageUrl);
    Task<List<Signature>> BulkCreateSignaturesAsync(List<(string name, string title, string imageUrl)> items);
    Task UpdateSignatureAsync(Signature signature);
    Task<bool> DeleteSignatureAsync(int id);
}
