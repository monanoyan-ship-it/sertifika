using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Participants;

public class ParticipantCrudFactory : IParticipantCrudFactory
{
    private readonly IParticipantEntityService _participantService;
    private readonly IUnitOfWork _unitOfWork;

    public ParticipantCrudFactory(IParticipantEntityService participantService, IUnitOfWork unitOfWork)
    {
        _participantService = participantService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Participant>> GetParticipantsByTrainingAsync(int trainingId)
        => await _participantService.GetByTrainingIdAsync(trainingId);

    public async Task<Participant?> GetParticipantAsync(int id)
        => await _participantService.GetByIdAsync(id);

    public async Task<Participant> CreateParticipantAsync(Participant participant)
    {
        _participantService.Add(participant);
        await _unitOfWork.SaveChangesAsync();
        return participant;
    }

    public async Task<int> ImportParticipantsAsync(int trainingId, List<ParticipantImportRow> rows)
    {
        var participants = rows.Select(r => new Participant
        {
            TrainingId = trainingId,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Email = r.Email,
            CompanyName = r.CompanyName
        });

        _participantService.AddRange(participants);
        await _unitOfWork.SaveChangesAsync();
        return rows.Count;
    }

    public async Task UpdateParticipantAsync(Participant participant)
    {
        participant.UpdatedAt = DateTime.UtcNow;
        _participantService.Update(participant);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteParticipantAsync(int id)
    {
        var participant = await _participantService.GetByIdAsync(id);
        if (participant == null) return false;
        participant.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
