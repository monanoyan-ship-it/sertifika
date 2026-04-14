using Sertifika.Entities;

namespace Sertifika.Factories.Participants;

public interface IParticipantCrudFactory
{
    Task<IEnumerable<Participant>> GetParticipantsByTrainingAsync(int trainingId);
    Task<Participant?> GetParticipantAsync(int id);
    Task<Participant> CreateParticipantAsync(Participant participant);
    Task<int> ImportParticipantsAsync(int trainingId, List<ParticipantImportRow> rows);
    Task<int> ImportFromExcelAsync(int trainingId, Stream excelStream);
    Task UpdateParticipantAsync(Participant participant);
    Task<bool> DeleteParticipantAsync(int id);
    byte[] BuildExcelTemplate();
}

public class ParticipantImportRow
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? CompanyName { get; set; }
}
