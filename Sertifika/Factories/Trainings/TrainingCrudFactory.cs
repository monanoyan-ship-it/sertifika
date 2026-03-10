using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Trainings;

public class TrainingCrudFactory : ITrainingCrudFactory
{
    private readonly ITrainingEntityService _trainingService;
    private readonly ITemplateEntityService _templateService;
    private readonly IUnitOfWork _unitOfWork;

    public TrainingCrudFactory(ITrainingEntityService trainingService, ITemplateEntityService templateService, IUnitOfWork unitOfWork)
    {
        _trainingService = trainingService;
        _templateService = templateService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Training>> GetTrainingsAsync()
        => await _trainingService.GetActiveTrainingsAsync();

    public async Task<Training?> GetTrainingAsync(int id)
        => await _trainingService.GetByIdWithDetailsAsync(id);

    public async Task<Training> CreateTrainingAsync(Training training)
    {
        var template = await _templateService.GetByIdWithSignaturesAsync(training.TemplateId);
        if (template?.TemplateSignatures != null)
        {
            foreach (var ts in template.TemplateSignatures.OrderBy(s => s.DisplayOrder))
            {
                training.TrainingSignatures.Add(new TrainingSignature
                {
                    SignatureId = ts.SignatureId,
                    DisplayOrder = ts.DisplayOrder,
                    InstructorName = ts.InstructorName,
                    InstructorTitle = ts.InstructorTitle,
                    ShowName = ts.ShowName,
                    ShowTitle = ts.ShowTitle,
                    ImageX = ts.ImageX,
                    ImageY = ts.ImageY,
                    ImageWidth = ts.ImageWidth,
                    ImageHeight = ts.ImageHeight,
                    ImageRotation = ts.ImageRotation,
                    NameX = ts.NameX,
                    NameY = ts.NameY,
                    TitleX = ts.TitleX,
                    TitleY = ts.TitleY
                });
            }
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
