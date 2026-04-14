using Sertifika.Entities;

namespace Sertifika.Factories.CertificateGeneration;

public interface ICertificateGenerationFactory
{
    Task<GenerationResult> GenerateCertificatesAsync(int trainingId);
    Task<byte[]> PreviewCertificateAsync(int trainingId, int? participantId = null);
    Task<byte[]> DownloadZipAsync(int trainingId);
    Task<byte[]?> RenderFromSnapshotAsync(string certificateNumber);
    Task<byte[]?> RenderByParticipantAsync(int participantId);
}

public class GenerationResult
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
