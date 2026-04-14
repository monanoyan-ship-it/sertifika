namespace Sertifika.Entities;

public class Training : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public DateTime TrainingDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CompanyName { get; set; }
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
    public string? InstructorName { get; set; }
    public string? InstructorTitle { get; set; }
    public TrainingStatus Status { get; set; } = TrainingStatus.Draft;

    public int TemplateId { get; set; }
    public CertificateTemplate Template { get; set; } = null!;

    public ICollection<TrainingSignature> TrainingSignatures { get; set; } = new List<TrainingSignature>();
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();

    public string FormatDateRange()
    {
        var start = TrainingDate;
        var end = EndDate;
        if (end == null || end.Value.Date == start.Date)
            return start.ToString("dd.MM.yyyy");
        if (end.Value.Year == start.Year && end.Value.Month == start.Month)
            return $"{start:dd}-{end.Value:dd}.{start:MM.yyyy}";
        if (end.Value.Year == start.Year)
            return $"{start:dd.MM}-{end.Value:dd.MM}.{start:yyyy}";
        return $"{start:dd.MM.yyyy}-{end.Value:dd.MM.yyyy}";
    }
}

public class TrainingSignature : BaseEntity
{
    public int TrainingId { get; set; }
    public Training Training { get; set; } = null!;

    public int SignatureId { get; set; }
    public Signature Signature { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public string? InstructorName { get; set; }
    public string? InstructorTitle { get; set; }

    public bool ShowName { get; set; } = true;
    public bool ShowTitle { get; set; } = true;

    public double ImageX { get; set; }
    public double ImageY { get; set; }
    public double ImageWidth { get; set; } = 12;
    public double ImageHeight { get; set; } = 8;
    public int ImageRotation { get; set; }
    public double NameX { get; set; }
    public double NameY { get; set; }
    public double TitleX { get; set; }
    public double TitleY { get; set; }
    public int NameFontSize { get; set; } = 8;
    public int TitleFontSize { get; set; } = 7;
}

public enum TrainingStatus
{
    Draft = 0,
    Ready = 1,
    Generated = 2,
    Distributed = 3
}
