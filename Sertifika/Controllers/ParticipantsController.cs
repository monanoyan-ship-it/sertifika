using Sertifika.Entities;
using Sertifika.Factories.Participants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/trainings/{trainingId}/[controller]")]
[Authorize]
public class ParticipantsController : ControllerBase
{
    private readonly IParticipantCrudFactory _crud;

    public ParticipantsController(IParticipantCrudFactory crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Participant>>> GetParticipants(int trainingId)
    {
        return Ok(await _crud.GetParticipantsByTrainingAsync(trainingId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Participant>> GetParticipant(int trainingId, int id)
    {
        var participant = await _crud.GetParticipantAsync(id);
        if (participant == null) return NotFound();
        return Ok(participant);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<Participant>> CreateParticipant(int trainingId, [FromBody] ParticipantUpsertRequest request)
    {
        var participant = new Participant
        {
            TrainingId = trainingId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            CompanyName = request.CompanyName
        };
        var created = await _crud.CreateParticipantAsync(participant);
        return CreatedAtAction(nameof(GetParticipant), new { trainingId, id = created.Id }, created);
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> ImportParticipants(int trainingId, [FromBody] List<ParticipantImportRow> rows)
    {
        if (rows == null || rows.Count == 0)
            return BadRequest(new { message = "Katilimci listesi bos." });

        var count = await _crud.ImportParticipantsAsync(trainingId, rows);
        return Ok(new { message = $"{count} katilimci eklendi.", count });
    }

    [HttpPost("import-excel")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> ImportFromExcel(int trainingId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya bos." });

        using var stream = file.OpenReadStream();
        var count = await _crud.ImportFromExcelAsync(trainingId, stream);
        return Ok(new { message = $"{count} katilimci eklendi.", count });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateParticipant(int trainingId, int id, [FromBody] ParticipantUpsertRequest request)
    {
        var participant = new Participant
        {
            Id = id,
            TrainingId = trainingId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            CompanyName = request.CompanyName
        };
        await _crud.UpdateParticipantAsync(participant);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteParticipant(int trainingId, int id)
    {
        var found = await _crud.DeleteParticipantAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }

    [HttpGet("excel-template")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public IActionResult DownloadExcelTemplate()
    {
        var bytes = _crud.BuildExcelTemplate();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "katilimci_sablonu.xlsx");
    }
}

public class ParticipantUpsertRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? CompanyName { get; set; }
}
