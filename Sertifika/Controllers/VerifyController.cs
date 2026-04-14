using Sertifika.EntityServices;
using Sertifika.Factories.CertificateGeneration;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/certificates/verify")]
public class VerifyController : ControllerBase
{
    private readonly ICertificateSnapshotEntityService _snapshotService;
    private readonly ICertificateGenerationFactory _generation;

    public VerifyController(
        ICertificateSnapshotEntityService snapshotService,
        ICertificateGenerationFactory generation)
    {
        _snapshotService = snapshotService;
        _generation = generation;
    }

    [HttpGet("{certificateNumber}")]
    public async Task<IActionResult> Verify(string certificateNumber)
    {
        var snapshot = await _snapshotService.GetByCertificateNumberAsync(certificateNumber);
        if (snapshot == null)
            return NotFound(new { valid = false, message = "Sertifika bulunamadi" });

        var participant = snapshot.Participant;
        var training = participant.Training;

        return Ok(new
        {
            valid = true,
            certificate = new
            {
                certificateNumber = snapshot.CertificateNumber,
                holderName = $"{participant.FirstName} {participant.LastName}",
                trainingName = training.Name,
                trainingDate = training.FormatDateRange(),
                companyName = participant.CompanyName ?? training.CompanyName,
                generatedAt = snapshot.GeneratedAt,
                hasDownload = true
            }
        });
    }

    [HttpGet("~/api/certificates/download/{certificateNumber}")]
    public async Task<IActionResult> Download(string certificateNumber)
    {
        var pdfBytes = await _generation.RenderFromSnapshotAsync(certificateNumber);
        if (pdfBytes == null)
            return NotFound(new { error = "Sertifika bulunamadi" });

        var filename = $"{certificateNumber}.pdf";
        return File(pdfBytes, "application/pdf", filename);
    }
}
