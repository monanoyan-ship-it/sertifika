using ClosedXML.Excel;
using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Participants;

public class ParticipantCrudFactory : IParticipantCrudFactory
{
    private readonly IParticipantEntityService _participantService;
    private readonly IUnitOfWork _unitOfWork;

    public ParticipantCrudFactory(IParticipantEntityService participantService, IUnitOfWork unitOfWork)
    {
        _participantService = participantService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Participant>> GetParticipantsByTrainingAsync(int trainingId)
        => await _participantService.GetByTrainingIdAsync(trainingId);

    public async Task<Participant?> GetParticipantAsync(int id)
        => await _participantService.GetByIdAsync(id);

    public async Task<Participant> CreateParticipantAsync(Participant participant)
    {
        _participantService.Add(participant);
        await _unitOfWork.SaveChangesAsync();
        return participant;
    }

    public async Task<int> ImportParticipantsAsync(int trainingId, List<ParticipantImportRow> rows)
    {
        var participants = rows.Select(r => new Participant
        {
            TrainingId = trainingId,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Email = r.Email,
            CompanyName = r.CompanyName
        });

        _participantService.AddRange(participants);
        await _unitOfWork.SaveChangesAsync();
        return rows.Count;
    }

    public async Task<int> ImportFromExcelAsync(int trainingId, Stream excelStream)
    {
        using var workbook = new XLWorkbook(excelStream);
        var ws = workbook.Worksheets.First();
        var rows = new List<ParticipantImportRow>();

        var firstRow = ws.FirstRowUsed();
        var lastRow = ws.LastRowUsed();
        if (firstRow == null || lastRow == null) return 0;

        var startRow = firstRow.RowNumber();
        // Detect header row
        var firstCell = ws.Cell(startRow, 1).GetString().Trim().ToLowerInvariant();
        if (firstCell is "ad" or "name" or "firstname" or "adı" or "isim")
            startRow++;

        for (var r = startRow; r <= lastRow.RowNumber(); r++)
        {
            var firstName = ws.Cell(r, 1).GetString().Trim();
            var lastName = ws.Cell(r, 2).GetString().Trim();
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName)) continue;

            rows.Add(new ParticipantImportRow
            {
                FirstName = firstName,
                LastName = lastName,
                Email = ws.Cell(r, 3).GetString().Trim(),
                CompanyName = ws.Cell(r, 4).GetString().Trim()
            });
        }

        return await ImportParticipantsAsync(trainingId, rows);
    }

    public async Task UpdateParticipantAsync(Participant participant)
    {
        var existing = await _participantService.GetByIdAsync(participant.Id);
        if (existing == null) return;

        existing.FirstName = participant.FirstName;
        existing.LastName = participant.LastName;
        existing.Email = participant.Email;
        existing.CompanyName = participant.CompanyName;
        existing.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteParticipantAsync(int id)
    {
        var participant = await _participantService.GetByIdAsync(id);
        if (participant == null) return false;
        participant.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public byte[] BuildExcelTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Katilimcilar");

        ws.Cell(1, 1).Value = "Ad";
        ws.Cell(1, 2).Value = "Soyad";
        ws.Cell(1, 3).Value = "Email";
        ws.Cell(1, 4).Value = "Firma";

        var header = ws.Range(1, 1, 1, 4);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        ws.Cell(2, 1).Value = "Ahmet";
        ws.Cell(2, 2).Value = "Yilmaz";
        ws.Cell(2, 3).Value = "ahmet@firma.com";
        ws.Cell(2, 4).Value = "ABC Ltd.";

        ws.Cell(3, 1).Value = "Ayse";
        ws.Cell(3, 2).Value = "Demir";
        ws.Cell(3, 3).Value = "ayse@firma.com";
        ws.Cell(3, 4).Value = "XYZ A.S.";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
