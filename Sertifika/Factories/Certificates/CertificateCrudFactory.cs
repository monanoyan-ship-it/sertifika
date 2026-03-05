using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Certificates;

public class CertificateCrudFactory : ICertificateCrudFactory
{
    private readonly ICertificateEntityService _certificateService;
    private readonly IUnitOfWork _unitOfWork;

    public CertificateCrudFactory(ICertificateEntityService certificateService, IUnitOfWork unitOfWork)
    {
        _certificateService = certificateService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Certificate>> GetCertificatesAsync()
    {
        return await _certificateService.GetActiveCertificatesAsync();
    }

    public async Task<Certificate?> GetCertificateAsync(int id)
    {
        return await _certificateService.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Certificate>> GetCertificatesByHolderAsync(int holderId)
    {
        return await _certificateService.GetByHolderIdAsync(holderId);
    }

    public async Task<IEnumerable<Certificate>> GetCertificatesByCategoryAsync(int categoryId)
    {
        return await _certificateService.GetByCategoryIdAsync(categoryId);
    }

    public async Task<Certificate> CreateCertificateAsync(Certificate certificate)
    {
        _certificateService.Add(certificate);
        await _unitOfWork.SaveChangesAsync();
        return certificate;
    }

    public async Task UpdateCertificateAsync(Certificate certificate)
    {
        certificate.UpdatedAt = DateTime.UtcNow;
        _certificateService.Update(certificate);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteCertificateAsync(int id)
    {
        var certificate = await _certificateService.GetByIdAsync(id);
        if (certificate == null) return false;

        certificate.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
