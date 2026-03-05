using Sertifika.Entities;

namespace Sertifika.Factories.Trainings;

public interface ITrainingCrudFactory
{
    Task<IEnumerable<Training>> GetTrainingsAsync();
    Task<Training?> GetTrainingAsync(int id);
    Task<Training> CreateTrainingAsync(Training training, List<int> signatureIds);
    Task UpdateTrainingAsync(Training training);
    Task<bool> DeleteTrainingAsync(int id);
}
