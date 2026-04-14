using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class CertificateSnapshotEntityService : ICertificateSnapshotEntityService
{
    private readonly AppDbContext _context;

    public CertificateSnapshotEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CertificateSnapshot?> GetByParticipantIdAsync(int participantId)
        => await _context.CertificateSnapshots
            .Include(s => s.Participant).ThenInclude(p => p.Training)
            .FirstOrDefaultAsync(s => s.ParticipantId == participantId);

    public async Task<CertificateSnapshot?> GetByCertificateNumberAsync(string certificateNumber)
        => await _context.CertificateSnapshots
            .Include(s => s.Participant).ThenInclude(p => p.Training)
            .FirstOrDefaultAsync(s => s.CertificateNumber == certificateNumber);

    public async Task<IEnumerable<CertificateSnapshot>> GetByTrainingIdAsync(int trainingId)
        => await _context.CertificateSnapshots
            .Include(s => s.Participant)
            .Where(s => s.Participant.TrainingId == trainingId)
            .ToListAsync();

    public void Add(CertificateSnapshot snapshot) => _context.CertificateSnapshots.Add(snapshot);
    public void Update(CertificateSnapshot snapshot) => _context.Entry(snapshot).State = EntityState.Modified;
}
