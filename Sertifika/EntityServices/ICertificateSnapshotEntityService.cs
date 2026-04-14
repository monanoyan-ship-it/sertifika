using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface ICertificateSnapshotEntityService
{
    Task<CertificateSnapshot?> GetByParticipantIdAsync(int participantId);
    Task<CertificateSnapshot?> GetByCertificateNumberAsync(string certificateNumber);
    Task<IEnumerable<CertificateSnapshot>> GetByTrainingIdAsync(int trainingId);
    void Add(CertificateSnapshot snapshot);
    void Update(CertificateSnapshot snapshot);
}
