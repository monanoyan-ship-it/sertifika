using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface IParticipantEntityService
{
    Task<IEnumerable<Participant>> GetByTrainingIdAsync(int trainingId);
    Task<Participant?> GetByIdAsync(int id);
    Task<Participant?> GetByCertificateNumberAsync(string certificateNumber);
    void Add(Participant participant);
    void AddRange(IEnumerable<Participant> participants);
    void Update(Participant participant);
}
