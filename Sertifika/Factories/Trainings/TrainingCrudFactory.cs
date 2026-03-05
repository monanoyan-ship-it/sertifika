using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Trainings;

public class TrainingCrudFactory : ITrainingCrudFactory
{
    private readonly ITrainingEntityService _trainingService;
    private readonly IUnitOfWork _unitOfWork;

    public TrainingCrudFactory(ITrainingEntityService trainingService, IUnitOfWork unitOfWork)
    {
        _trainingService = trainingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Training>> GetTrainingsAsync()
        => await _trainingService.GetActiveTrainingsAsync();

    public async Task<Training?> GetTrainingAsync(int id)
        => await _trainingService.GetByIdWithDetailsAsync(id);

    public async Task<Training> CreateTrainingAsync(Training training, List<int> signatureIds)
    {
        for (int i = 0; i < signatureIds.Count; i++)
        {
            training.TrainingSignatures.Add(new TrainingSignature
            {
                SignatureId = signatureIds[i],
                DisplayOrder = i
            });
        }

        _trainingService.Add(training);
        await _unitOfWork.SaveChangesAsync();
        return training;
    }

    public async Task UpdateTrainingAsync(Training training)
    {
        training.UpdatedAt = DateTime.UtcNow;
        _trainingService.Update(training);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteTrainingAsync(int id)
    {
        var training = await _trainingService.GetByIdAsync(id);
        if (training == null) return false;
        training.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
