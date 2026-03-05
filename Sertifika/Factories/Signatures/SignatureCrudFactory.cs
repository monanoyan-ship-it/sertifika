using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Signatures;

public class SignatureCrudFactory : ISignatureCrudFactory
{
    private readonly ISignatureEntityService _signatureService;
    private readonly IUnitOfWork _unitOfWork;

    public SignatureCrudFactory(ISignatureEntityService signatureService, IUnitOfWork unitOfWork)
    {
        _signatureService = signatureService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Signature>> GetSignaturesAsync()
        => await _signatureService.GetActiveSignaturesAsync();

    public async Task<Signature?> GetSignatureAsync(int id)
        => await _signatureService.GetByIdAsync(id);

    public async Task<Signature> CreateSignatureAsync(string name, string title, string imageUrl)
    {
        var signature = new Signature
        {
            Name = name,
            Title = title,
            ImageUrl = imageUrl
        };
        _signatureService.Add(signature);
        await _unitOfWork.SaveChangesAsync();
        return signature;
    }

    public async Task UpdateSignatureAsync(Signature signature)
    {
        signature.UpdatedAt = DateTime.UtcNow;
        _signatureService.Update(signature);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteSignatureAsync(int id)
    {
        var signature = await _signatureService.GetByIdAsync(id);
        if (signature == null) return false;
        signature.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
