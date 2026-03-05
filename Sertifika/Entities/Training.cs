namespace Sertifika.Entities;

public class Training : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime TrainingDate { get; set; }
    public string? CompanyName { get; set; }
    public TrainingStatus Status { get; set; } = TrainingStatus.Draft;

    public int TemplateId { get; set; }
    public CertificateTemplate Template { get; set; } = null!;

    public ICollection<TrainingSignature> TrainingSignatures { get; set; } = new List<TrainingSignature>();
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
}

public class TrainingSignature : BaseEntity
{
    public int TrainingId { get; set; }
    public Training Training { get; set; } = null!;

    public int SignatureId { get; set; }
    public Signature Signature { get; set; } = null!;

    public int DisplayOrder { get; set; }
}

public enum TrainingStatus
{
    Draft = 0,
    Ready = 1,
    Generated = 2,
    Distributed = 3
}
