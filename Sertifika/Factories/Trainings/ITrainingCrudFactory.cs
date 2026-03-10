using Sertifika.Entities;

namespace Sertifika.Factories.Trainings;

public interface ITrainingCrudFactory
{
    Task<IEnumerable<Training>> GetTrainingsAsync();
    Task<Training?> GetTrainingAsync(int id);
    Task<Training> CreateTrainingAsync(Training training);
    Task UpdateTrainingAsync(Training training);
    Task<bool> DeleteTrainingAsync(int id);
}
