using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class ParticipantEntityService : IParticipantEntityService
{
    private readonly AppDbContext _context;

    public ParticipantEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Participant>> GetByTrainingIdAsync(int trainingId)
        => await _context.Participants
            .Where(p => p.IsActive && p.TrainingId == trainingId)
            .ToListAsync();

    public async Task<Participant?> GetByIdAsync(int id)
        => await _context.Participants.FindAsync(id);

    public void Add(Participant participant) => _context.Participants.Add(participant);

    public void AddRange(IEnumerable<Participant> participants) => _context.Participants.AddRange(participants);

    public void Update(Participant participant) => _context.Entry(participant).State = EntityState.Modified;
}
