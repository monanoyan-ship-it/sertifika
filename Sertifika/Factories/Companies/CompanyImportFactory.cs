using System.Globalization;
using System.Text;
using Sertifika.Entities;
using Sertifika.EntityServices;
using Sertifika.Infrastructure;

namespace Sertifika.Factories.Companies;

public class CompanyImportFactory : ICompanyImportFactory
{
    private readonly ICompanyEntityService _companyService;
    private readonly IContactEntityService _contactService;
    private readonly ITrainingEntityService _trainingService;
    private readonly IParticipantEntityService _participantService;
    private readonly ITemplateEntityService _templateService;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly string[] DateFormats =
    {
        "dd.MM.yyyy", "dd/MM/yyyy", "dd-MM-yyyy",
        "yyyy-MM-dd", "yyyy/MM/dd",
        "d.M.yyyy", "d/M/yyyy"
    };

    public CompanyImportFactory(
        ICompanyEntityService companyService,
        IContactEntityService contactService,
        ITrainingEntityService trainingService,
        IParticipantEntityService participantService,
        ITemplateEntityService templateService,
        IUnitOfWork unitOfWork)
    {
        _companyService = companyService;
        _contactService = contactService;
        _trainingService = trainingService;
        _participantService = participantService;
        _templateService = templateService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportPreviewResult> PreviewAsync(Stream csvStream)
    {
        var result = new ImportPreviewResult();
        var rows = ParseCsv(csvStream, result.Errors);
        result.TotalRows = rows.Count;
        if (rows.Count == 0) return result;

        var seenCompanies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenTrainingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenContactKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var company = await _companyService.FindByNameAsync(row.CompanyName);
            var companyId = company?.Id;
            if (company == null && seenCompanies.Add(row.CompanyName))
                result.NewCompanies.Add(row.CompanyName);

            var trainingKey = $"{row.CompanyName}|{row.TrainingName}|{row.TrainingDate:yyyyMMdd}";
            Training? existingTraining = null;
            if (companyId.HasValue)
                existingTraining = await _trainingService.FindByCompanyNameAndDateAsync(companyId.Value, row.TrainingName, row.TrainingDate);

            if (existingTraining == null && seenTrainingKeys.Add(trainingKey))
            {
                result.NewTrainings.Add(new PreviewTraining
                {
                    CompanyName = row.CompanyName,
                    Name = row.TrainingName,
                    TrainingDate = row.TrainingDate,
                    EndDate = row.EndDate,
                    InstructorName = row.InstructorName
                });
            }

            var contactKey = $"{row.CompanyName}|{row.Email ?? row.FirstName + row.LastName}".ToLowerInvariant();
            Contact? existingContact = null;
            if (companyId.HasValue && !string.IsNullOrWhiteSpace(row.Email))
                existingContact = await _contactService.FindByCompanyAndEmailAsync(companyId.Value, row.Email);

            if (existingContact == null && seenContactKeys.Add(contactKey))
            {
                result.NewContacts.Add(new PreviewContact
                {
                    CompanyName = row.CompanyName,
                    FullName = $"{row.FirstName} {row.LastName}",
                    Email = row.Email
                });
            }

            // Participation check
            if (existingTraining != null && existingContact != null)
            {
                var participants = await _participantService.GetByTrainingIdAsync(existingTraining.Id);
                if (participants.Any(p => p.ContactId == existingContact.Id))
                {
                    result.ExistingParticipations++;
                    continue;
                }
            }
            result.NewParticipations++;
        }

        return result;
    }

    public async Task<ImportConfirmResult> ConfirmAsync(Stream csvStream, int defaultTemplateId)
    {
        var result = new ImportConfirmResult();
        var template = await _templateService.GetByIdAsync(defaultTemplateId);
        if (template == null)
        {
            result.Warnings.Add("Secilen sablon bulunamadi. Import iptal edildi.");
            return result;
        }

        var errors = new List<string>();
        var rows = ParseCsv(csvStream, errors);
        if (errors.Count > 0)
        {
            result.Warnings.AddRange(errors);
            if (rows.Count == 0) return result;
        }

        var companyCache = new Dictionary<string, Company>(StringComparer.OrdinalIgnoreCase);
        var contactCache = new Dictionary<(int companyId, string email), Contact>();
        var trainingCache = new Dictionary<(int companyId, string name, DateTime date), Training>();

        foreach (var row in rows)
        {
            try
            {
                // Company
                if (!companyCache.TryGetValue(row.CompanyName, out var company))
                {
                    company = await _companyService.FindByNameAsync(row.CompanyName) ?? new Company
                    {
                        Name = row.CompanyName.Trim()
                    };
                    if (company.Id == 0)
                    {
                        _companyService.Add(company);
                        await _unitOfWork.SaveChangesAsync();
                        result.CompaniesCreated++;
                    }
                    companyCache[row.CompanyName] = company;
                }

                // Training
                var trainingKey = (company.Id, row.TrainingName.Trim().ToLowerInvariant(), row.TrainingDate.Date);
                if (!trainingCache.TryGetValue(trainingKey, out var training))
                {
                    training = await _trainingService.FindByCompanyNameAndDateAsync(company.Id, row.TrainingName, row.TrainingDate);
                    if (training == null)
                    {
                        training = new Training
                        {
                            Name = row.TrainingName.Trim(),
                            TrainingDate = DateTime.SpecifyKind(row.TrainingDate.Date, DateTimeKind.Utc),
                            EndDate = row.EndDate.HasValue
                                ? DateTime.SpecifyKind(row.EndDate.Value.Date, DateTimeKind.Utc)
                                : null,
                            CompanyName = company.Name,
                            CompanyId = company.Id,
                            InstructorName = string.IsNullOrWhiteSpace(row.InstructorName) ? null : row.InstructorName.Trim(),
                            TemplateId = defaultTemplateId,
                            Status = TrainingStatus.Draft
                        };
                        _trainingService.Add(training);
                        await _unitOfWork.SaveChangesAsync();
                        result.TrainingsCreated++;
                    }
                    trainingCache[trainingKey] = training;
                }

                // Contact
                Contact? contact = null;
                if (!string.IsNullOrWhiteSpace(row.Email))
                {
                    var contactKey = (company.Id, row.Email.Trim().ToLowerInvariant());
                    if (!contactCache.TryGetValue(contactKey, out contact))
                    {
                        contact = await _contactService.FindByCompanyAndEmailAsync(company.Id, row.Email);
                        if (contact == null)
                        {
                            contact = new Contact
                            {
                                FirstName = row.FirstName.Trim(),
                                LastName = row.LastName.Trim(),
                                Email = row.Email.Trim(),
                                Phone = string.IsNullOrWhiteSpace(row.Phone) ? null : row.Phone.Trim(),
                                CompanyId = company.Id
                            };
                            _contactService.Add(contact);
                            await _unitOfWork.SaveChangesAsync();
                            result.ContactsCreated++;
                        }
                        contactCache[contactKey] = contact;
                    }
                }

                // Participant (idempotent: ayni Contact+Training varsa atla)
                var existingParticipants = await _participantService.GetByTrainingIdAsync(training.Id);
                if (contact != null && existingParticipants.Any(p => p.ContactId == contact.Id))
                {
                    result.RowsSkipped++;
                    continue;
                }

                var participant = new Participant
                {
                    TrainingId = training.Id,
                    ContactId = contact?.Id,
                    FirstName = row.FirstName.Trim(),
                    LastName = row.LastName.Trim(),
                    Email = string.IsNullOrWhiteSpace(row.Email) ? null : row.Email.Trim(),
                    CompanyName = company.Name
                };
                _participantService.Add(participant);
                await _unitOfWork.SaveChangesAsync();
                result.ParticipantsCreated++;
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Satir {row.LineNumber}: {ex.Message}");
                result.RowsSkipped++;
            }
        }

        return result;
    }

    // ─── CSV Parser ───

    private static List<CsvRow> ParseCsv(Stream stream, List<string> errors)
    {
        var rows = new List<CsvRow>();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? line;
        var lineNum = 0;
        var headerLine = reader.ReadLine();
        lineNum++;
        if (headerLine == null)
        {
            errors.Add("CSV dosyasi bos.");
            return rows;
        }

        var separator = DetectSeparator(headerLine);
        var headers = SplitCsvLine(headerLine, separator).Select(Normalize).ToList();

        var idx = new Dictionary<string, int>();
        for (var i = 0; i < headers.Count; i++) idx[headers[i]] = i;

        int? F(params string[] keys) => keys.Select(k => idx.TryGetValue(Normalize(k), out var i) ? i : (int?)null).FirstOrDefault(v => v.HasValue);
        var iFirstName = F("ad", "adi", "firstname", "name");
        var iLastName = F("soyad", "soyadi", "lastname", "surname");
        var iEmail = F("email", "eposta", "e-posta", "mail");
        var iPhone = F("telefon", "phone", "gsm");
        var iCompany = F("firma", "sirket", "company");
        var iTraining = F("egitim", "egitimadi", "egitimiadi", "training", "trainingname");
        var iInstructor = F("egitmen", "egitmenadi", "instructor", "instructoradi");
        var iDate = F("tarih", "baslangic", "baslangictarihi", "date", "startdate");
        var iEndDate = F("bitis", "bitistarihi", "enddate");

        var required = new (int? Index, string Name)[]
        {
            (iFirstName, "Ad"),
            (iLastName, "Soyad"),
            (iCompany, "Firma"),
            (iTraining, "EgitimAdi"),
            (iDate, "BaslangicTarihi")
        };
        foreach (var r in required)
        {
            if (!r.Index.HasValue)
            {
                errors.Add($"Zorunlu kolon eksik: {r.Name}");
                return rows;
            }
        }

        while ((line = reader.ReadLine()) != null)
        {
            lineNum++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cells = SplitCsvLine(line, separator);
            string Get(int? i) => i.HasValue && i.Value < cells.Count ? cells[i.Value].Trim() : string.Empty;

            var firstName = Get(iFirstName);
            var lastName = Get(iLastName);
            var company = Get(iCompany);
            var trainingName = Get(iTraining);
            var dateRaw = Get(iDate);

            if (string.IsNullOrWhiteSpace(firstName) &&
                string.IsNullOrWhiteSpace(lastName) &&
                string.IsNullOrWhiteSpace(company)) continue;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                errors.Add($"Satir {lineNum}: Ad/Soyad bos.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(company))
            {
                errors.Add($"Satir {lineNum}: Firma bos.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(trainingName))
            {
                errors.Add($"Satir {lineNum}: Egitim adi bos.");
                continue;
            }
            if (!TryParseDate(dateRaw, out var trainingDate))
            {
                errors.Add($"Satir {lineNum}: Tarih okunamadi ('{dateRaw}').");
                continue;
            }
            DateTime? endDate = null;
            var endRaw = Get(iEndDate);
            if (!string.IsNullOrWhiteSpace(endRaw))
            {
                if (TryParseDate(endRaw, out var parsed)) endDate = parsed;
                else errors.Add($"Satir {lineNum}: Bitis tarihi okunamadi ('{endRaw}') - yok sayildi.");
            }

            rows.Add(new CsvRow
            {
                FirstName = firstName,
                LastName = lastName,
                Email = NullIfEmpty(Get(iEmail)),
                Phone = NullIfEmpty(Get(iPhone)),
                CompanyName = company,
                TrainingName = trainingName,
                InstructorName = NullIfEmpty(Get(iInstructor)),
                TrainingDate = trainingDate,
                EndDate = endDate,
                LineNumber = lineNum
            });
        }

        return rows;
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static string Normalize(string s) =>
        (s ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
            .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c")
            .Replace(" ", "").Replace("_", "").Replace("-", "");

    private static char DetectSeparator(string header)
    {
        if (header.Count(c => c == ';') > header.Count(c => c == ',')) return ';';
        if (header.Contains('\t')) return '\t';
        return ',';
    }

    private static List<string> SplitCsvLine(string line, char sep)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == sep && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString());
        return result;
    }

    private static bool TryParseDate(string raw, out DateTime date)
    {
        if (DateTime.TryParseExact(raw, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return true;
        return DateTime.TryParse(raw, CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out date)
            || DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }
}
