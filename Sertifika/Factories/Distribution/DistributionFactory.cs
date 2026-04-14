using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Factories.CertificateGeneration;
using Sertifika.Infrastructure;
using Sertifika.Services;

namespace Sertifika.Factories.Distribution;

public class DistributionFactory : IDistributionFactory
{
    private readonly ITrainingEntityService _trainingService;
    private readonly IParticipantEntityService _participantService;
    private readonly ICertificateSnapshotEntityService _snapshotService;
    private readonly ICertificateGenerationFactory _generation;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public DistributionFactory(
        ITrainingEntityService trainingService,
        IParticipantEntityService participantService,
        ICertificateSnapshotEntityService snapshotService,
        ICertificateGenerationFactory generation,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _trainingService = trainingService;
        _participantService = participantService;
        _snapshotService = snapshotService;
        _generation = generation;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmailBatchResult> SendCertificatesToParticipantsAsync(int trainingId)
    {
        var training = await _trainingService.GetByIdWithDetailsAsync(trainingId);
        if (training == null)
            throw new ArgumentException("Training not found");

        var participants = await _participantService.GetByTrainingIdAsync(trainingId);
        var recipients = new List<EmailRecipient>();

        foreach (var p in participants)
        {
            if (string.IsNullOrEmpty(p.Email) || string.IsNullOrEmpty(p.CertificateNumber))
                continue;

            var pdfBytes = await _generation.RenderByParticipantAsync(p.Id);
            if (pdfBytes == null) continue;

            recipients.Add(new EmailRecipient
            {
                Email = p.Email,
                Name = $"{p.FirstName} {p.LastName}",
                PdfBytes = pdfBytes,
                PdfFilename = $"{p.CertificateNumber}.pdf",
                CertificateNo = p.CertificateNumber
            });
        }

        if (recipients.Count == 0)
            throw new InvalidOperationException("Sertifikasi uretilmis ve e-postasi dolu katilimci yok. Once 'Sertifika Uret' butonuna basin ve katilimcilarin e-postalarini kontrol edin.");

        var result = await _emailService.SendBatchAsync(recipients, training.Name, training.CompanyName);

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

        foreach (var p in participants)
        {
            if (string.IsNullOrEmpty(p.CertificateNumber)) continue;

            var pdfBytes = await _generation.RenderByParticipantAsync(p.Id);
            if (pdfBytes == null) continue;

            await _emailService.SendCertificateEmailAsync(
                email, name, training.Name,
                pdfBytes, $"{p.CertificateNumber}.pdf",
                training.CompanyName, p.CertificateNumber);
        }
    }
}
