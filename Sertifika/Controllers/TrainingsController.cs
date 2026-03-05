using Sertifika.Entities;
using Sertifika.Factories.Trainings;
using Sertifika.Factories.CertificateGeneration;
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

    public TrainingsController(ITrainingCrudFactory crud, ICertificateGenerationFactory generation)
    {
        _crud = crud;
        _generation = generation;
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
            Description = request.Description,
            TrainingDate = request.TrainingDate,
            CompanyName = request.CompanyName,
            TemplateId = request.TemplateId
        };

        var created = await _crud.CreateTrainingAsync(training, request.SignatureIds);
        return CreatedAtAction(nameof(GetTraining), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateTraining(int id, Training training)
    {
        if (id != training.Id) return BadRequest();
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
}

public class CreateTrainingRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime TrainingDate { get; set; }
    public string? CompanyName { get; set; }
    public int TemplateId { get; set; }
    public List<int> SignatureIds { get; set; } = new();
}
