using Sertifika.EntityServices;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/certificates/verify")]
public class VerifyController : ControllerBase
{
    private readonly IParticipantEntityService _participantService;

    public VerifyController(IParticipantEntityService participantService)
    {
        _participantService = participantService;
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
                companyName = participant.CompanyName ?? participant.Training.CompanyName
            }
        });
    }
}
