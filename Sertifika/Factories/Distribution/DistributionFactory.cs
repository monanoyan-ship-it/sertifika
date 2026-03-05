using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;
using Sertifika.Services;

namespace Sertifika.Factories.Distribution;

public class DistributionFactory : IDistributionFactory
{
    private readonly ITrainingEntityService _trainingService;
    private readonly IParticipantEntityService _participantService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _env;

    public DistributionFactory(
        ITrainingEntityService trainingService,
        IParticipantEntityService participantService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IWebHostEnvironment env)
    {
        _trainingService = trainingService;
        _participantService = participantService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _env = env;
    }

    public async Task<EmailBatchResult> SendCertificatesToParticipantsAsync(int trainingId)
    {
        var training = await _trainingService.GetByIdWithDetailsAsync(trainingId);
        if (training == null)
            throw new ArgumentException("Training not found");

        var participants = await _participantService.GetByTrainingIdAsync(trainingId);
        var recipients = new List<EmailRecipient>();

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

        foreach (var p in participants)
        {
            if (string.IsNullOrEmpty(p.Email) || string.IsNullOrEmpty(p.CertificatePdfUrl))
                continue;

            var pdfPath = Path.Combine(webRoot, p.CertificatePdfUrl.TrimStart('/'));
            if (!File.Exists(pdfPath))
                continue;

            recipients.Add(new EmailRecipient
            {
                Email = p.Email,
                Name = $"{p.FirstName} {p.LastName}",
                PdfFilePath = pdfPath
            });
        }

        if (recipients.Count == 0)
            throw new InvalidOperationException("No participants with email and generated certificates found");

        var result = await _emailService.SendBatchAsync(recipients, training.Name);

        training.Status = TrainingStatus.Distributed;
        training.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task SendToContactAsync(int trainingId, string email, string name)
    {
        var training = await _trainingService.GetByIdWithDetailsAsync(trainingId);
        if (training == null)
            throw new ArgumentException("Training not found");

        var participants = await _participantService.GetByTrainingIdAsync(trainingId);
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

        foreach (var p in participants)
        {
            if (string.IsNullOrEmpty(p.CertificatePdfUrl))
                continue;

            var pdfPath = Path.Combine(webRoot, p.CertificatePdfUrl.TrimStart('/'));
            if (!File.Exists(pdfPath))
                continue;

            await _emailService.SendCertificateEmailAsync(email, name, training.Name, pdfPath);
        }
    }
}
