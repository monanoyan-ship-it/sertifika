using Sertifika.Entities;

namespace Sertifika.EntityServices;

public interface ITrainingEntityService
{
    Task<IEnumerable<Training>> GetActiveTrainingsAsync();
    Task<Training?> GetByIdAsync(int id);
    Task<Training?> GetByIdWithDetailsAsync(int id);
    void Add(Training training);
    void Update(Training training);
}
