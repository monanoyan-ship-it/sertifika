using Sertifika.Services;

namespace Sertifika.Factories.Distribution;

public interface IDistributionFactory
{
    Task<EmailBatchResult> SendCertificatesToParticipantsAsync(int trainingId);
    Task SendToContactAsync(int trainingId, string email, string name);
}
