using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Factories.Trainings;
using Sertifika.Factories.CertificateGeneration;
using Sertifika.Factories.Distribution;
using Sertifika.Infrastructure;
using Sertifika.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainingsController : ControllerBase
{
    private readonly ITrainingCrudFactory _crud;
    private readonly ICertificateGenerationFactory _generation;
    private readonly IDistributionFactory _distribution;
    private readonly IOneDriveService _oneDrive;
    private readonly IParticipantEntityService _participantService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _env;

    public TrainingsController(
        ITrainingCrudFactory crud,
        ICertificateGenerationFactory generation,
        IDistributionFactory distribution,
        IOneDriveService oneDrive,
        IParticipantEntityService participantService,
        IUnitOfWork unitOfWork,
        IWebHostEnvironment env)
    {
        _crud = crud;
        _generation = generation;
        _distribution = distribution;
        _oneDrive = oneDrive;
        _participantService = participantService;
        _unitOfWork = unitOfWork;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Training>>> GetTrainings()
    {
        return Ok(await _crud.GetTrainingsAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Training>> GetTraining(int id)
    {
        var training = await _crud.GetTrainingAsync(id);
        if (training == null) return NotFound();
        return Ok(training);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<Training>> CreateTraining([FromBody] CreateTrainingRequest request)
    {
        var training = new Training
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            TrainingDate = request.TrainingDate,
            CompanyName = request.CompanyName,
            TemplateId = request.TemplateId
        };

        var created = await _crud.CreateTrainingAsync(training);
        return CreatedAtAction(nameof(GetTraining), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateTraining(int id, [FromBody] UpdateTrainingRequest request)
    {
        var training = await _crud.GetTrainingAsync(id);
        if (training == null) return NotFound();

        training.Name = request.Name;
        training.DisplayName = request.DisplayName;
        training.Description = request.Description;
        training.TrainingDate = request.TrainingDate;
        training.CompanyName = request.CompanyName;
        training.TemplateId = request.TemplateId;

        await _crud.UpdateTrainingAsync(training);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTraining(int id)
    {
        var found = await _crud.DeleteTrainingAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }
    [HttpPost("{id}/generate")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<GenerationResult>> GenerateCertificates(int id)
    {
        try
        {
            var result = await _generation.GenerateCertificatesAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/preview")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> PreviewCertificate(int id, [FromQuery] int? participantId = null)
    {
        try
        {
            var pdfBytes = await _generation.PreviewCertificateAsync(id, participantId);
            return File(pdfBytes, "application/pdf", "preview.pdf");
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/download-zip")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> DownloadZip(int id)
    {
        try
        {
            var zipBytes = await _generation.DownloadZipAsync(id);
            return File(zipBytes, "application/zip", $"certificates_training_{id}.zip");
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/send-certificates")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<EmailBatchResult>> SendCertificates(int id)
    {
        try
        {
            var result = await _distribution.SendCertificatesToParticipantsAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/send-to-contact")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> SendToContact(int id, [FromBody] SendToContactRequest request)
    {
        try
        {
            await _distribution.SendToContactAsync(id, request.Email, request.Name);
            return Ok(new { message = "Certificates sent successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/archive-onedrive")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OneDriveUploadResult>> ArchiveToOneDrive(int id)
    {
        try
        {
            var training = await _crud.GetTrainingAsync(id);
            if (training == null) return NotFound();

            var result = await _oneDrive.ArchiveTrainingCertificatesAsync(
                id, training.CompanyName ?? "Genel", training.Name, training.TrainingDate);

            // Update participants: set cloud file IDs and delete local files
            if (result.FileItemIds.Count > 0)
            {
                var participants = (await _participantService.GetByTrainingIdAsync(id)).ToList();
                var certDir = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
                    "uploads", "certificates", $"training_{id}");

                foreach (var participant in participants)
                {
                    if (string.IsNullOrEmpty(participant.CertificatePdfUrl)) continue;
                    var fileName = Path.GetFileName(participant.CertificatePdfUrl);
                    if (result.FileItemIds.TryGetValue(fileName, out var itemId))
                    {
                        participant.StorageType = CertificateStorageType.OneDrive;
                        participant.CloudFileId = itemId;
                        _participantService.Update(participant);

                        // Delete local file
                        var localPath = Path.Combine(certDir, fileName);
                        if (System.IO.File.Exists(localPath))
                            System.IO.File.Delete(localPath);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Remove directory if empty
                if (Directory.Exists(certDir) && !Directory.EnumerateFiles(certDir).Any())
                    Directory.Delete(certDir);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class SendToContactRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateTrainingRequest
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public DateTime TrainingDate { get; set; }
    public string? CompanyName { get; set; }
    public int TemplateId { get; set; }
}

public class UpdateTrainingRequest
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public DateTime TrainingDate { get; set; }
    public string? CompanyName { get; set; }
    public int TemplateId { get; set; }
}
