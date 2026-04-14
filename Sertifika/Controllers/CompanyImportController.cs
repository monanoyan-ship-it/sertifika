using System.Text;
using Sertifika.Factories.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/companies/import")]
[Authorize(Roles = "Admin,CertificateCreator")]
public class CompanyImportController : ControllerBase
{
    private readonly ICompanyImportFactory _import;

    public CompanyImportController(ICompanyImportFactory import)
    {
        _import = import;
    }

    [HttpPost("preview")]
    public async Task<ActionResult<ImportPreviewResult>> Preview(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Dosya bos." });

        using var stream = file.OpenReadStream();
        var result = await _import.PreviewAsync(stream);
        return Ok(result);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<ImportConfirmResult>> Confirm(IFormFile file, [FromForm] int templateId)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Dosya bos." });
        if (templateId <= 0)
            return BadRequest(new { error = "Varsayilan sablon secilmedi." });

        using var stream = file.OpenReadStream();
        var result = await _import.ConfirmAsync(stream, templateId);
        return Ok(result);
    }

    [HttpGet("template")]
    public IActionResult DownloadTemplate()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Ad;Soyad;Email;Telefon;Firma;EgitimAdi;Egitmen;BaslangicTarihi;BitisTarihi");
        sb.AppendLine("Ahmet;Yilmaz;ahmet@abc.com;5551234567;ABC Ltd.;Is Guvenligi Egitimi;Dr. Mehmet Kaya;10.04.2026;11.04.2026");
        sb.AppendLine("Ayse;Demir;ayse@abc.com;5559876543;ABC Ltd.;Is Guvenligi Egitimi;Dr. Mehmet Kaya;10.04.2026;11.04.2026");
        sb.AppendLine("Can;Ozturk;can@xyz.com;;XYZ A.S.;Yangin Egitimi;Ali Veli;15.05.2026;");

        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var bytes = bom.Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", "firma_import_sablonu.csv");
    }
}
