using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Services;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/certificates/verify")]
public class VerifyController : ControllerBase
{
    private readonly IParticipantEntityService _participantService;
    private readonly IOneDriveService _oneDrive;
    private readonly IWebHostEnvironment _env;

    public VerifyController(
        IParticipantEntityService participantService,
        IOneDriveService oneDrive,
        IWebHostEnvironment env)
    {
        _participantService = participantService;
        _oneDrive = oneDrive;
        _env = env;
    }

    [HttpGet("{certificateNumber}")]
    public async Task<IActionResult> Verify(string certificateNumber)
    {
        var participant = await _participantService.GetByCertificateNumberAsync(certificateNumber);

        if (participant == null)
            return NotFound(new { valid = false, message = "Sertifika bulunamadi" });

        return Ok(new
        {
            valid = true,
            certificate = new
            {
                certificateNumber = participant.CertificateNumber,
                holderName = $"{participant.FirstName} {participant.LastName}",
                trainingName = participant.Training.Name,
                trainingDate = participant.Training.TrainingDate.ToString("dd.MM.yyyy"),
                companyName = participant.CompanyName ?? participant.Training.CompanyName,
                hasDownload = !string.IsNullOrEmpty(participant.CertificatePdfUrl)
            }
        });
    }

    [HttpGet("~/api/certificates/download/{certificateNumber}")]
    public async Task<IActionResult> Download(string certificateNumber)
    {
        var participant = await _participantService.GetByCertificateNumberAsync(certificateNumber);
        if (participant == null || string.IsNullOrEmpty(participant.CertificatePdfUrl))
            return NotFound();

        byte[]? pdfBytes;

        if (participant.StorageType == CertificateStorageType.OneDrive && !string.IsNullOrEmpty(participant.CloudFileId))
        {
            pdfBytes = await _oneDrive.DownloadFileAsync(participant.CloudFileId);
            if (pdfBytes == null)
                return StatusCode(502, new { error = "Bulut depolamadan dosya alinamadi" });
        }
        else
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var localPath = Path.Combine(webRoot, participant.CertificatePdfUrl.TrimStart('/'));
            if (!System.IO.File.Exists(localPath))
                return NotFound(new { error = "Dosya bulunamadi" });
            pdfBytes = await System.IO.File.ReadAllBytesAsync(localPath);
        }

        var fileName = Path.GetFileName(participant.CertificatePdfUrl);
        return File(pdfBytes, "application/pdf", fileName);
    }
}
