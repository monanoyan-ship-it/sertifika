using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface ITrainingEntityService
{
    Task<IEnumerable<Training>> GetActiveTrainingsAsync();
    Task<Training?> GetByIdAsync(int id);
    Task<Training?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Training>> GetByCompanyIdAsync(int companyId);
    Task<Training?> FindByCompanyNameAndDateAsync(int companyId, string name, DateTime date);
    void Add(Training training);
    void Update(Training training);
}
