using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class TrainingEntityService : ITrainingEntityService
{
    private readonly AppDbContext _context;

    public TrainingEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Training>> GetActiveTrainingsAsync()
        => await _context.Trainings
            .Include(t => t.Template)
            .Include(t => t.TrainingSignatures).ThenInclude(ts => ts.Signature)
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.TrainingDate)
            .ToListAsync();

    public async Task<Training?> GetByIdAsync(int id)
        => await _context.Trainings.FindAsync(id);

    public async Task<Training?> GetByIdWithDetailsAsync(int id)
        => await _context.Trainings
            .Include(t => t.Template)
            .Include(t => t.TrainingSignatures).ThenInclude(ts => ts.Signature)
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Training>> GetByCompanyIdAsync(int companyId)
        => await _context.Trainings
            .Include(t => t.Template)
            .Include(t => t.Participants)
            .Where(t => t.IsActive && t.CompanyId == companyId)
            .OrderByDescending(t => t.TrainingDate)
            .ToListAsync();

    public async Task<Training?> FindByCompanyNameAndDateAsync(int companyId, string name, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var normalized = name.Trim().ToLowerInvariant();
        var day = date.Date;
        return await _context.Trainings
            .FirstOrDefaultAsync(t =>
                t.IsActive &&
                t.CompanyId == companyId &&
                t.Name.ToLower() == normalized &&
                t.TrainingDate.Date == day);
    }

    public void Add(Training training) => _context.Trainings.Add(training);

    public void Update(Training training) => _context.Entry(training).State = EntityState.Modified;
}
