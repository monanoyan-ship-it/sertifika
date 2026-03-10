using System.Text.Json;
using Sertifika.Entities;
using Sertifika.Factories.Signatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SignaturesController : ControllerBase
{
    private readonly ISignatureCrudFactory _crud;
    private readonly IWebHostEnvironment _env;

    public SignaturesController(ISignatureCrudFactory crud, IWebHostEnvironment env)
    {
        _crud = crud;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Signature>>> GetSignatures()
    {
        return Ok(await _crud.GetSignaturesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Signature>> GetSignature(int id)
    {
        var signature = await _crud.GetSignatureAsync(id);
        if (signature == null) return NotFound();
        return signature;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<Signature>> CreateSignature([FromForm] string name, [FromForm] string title, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Imza dosyasi gereklidir." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            return BadRequest(new { message = "Sadece PNG ve JPG dosyalari kabul edilir." });

        var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "signatures");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"/uploads/signatures/{fileName}";
        var created = await _crud.CreateSignatureAsync(name, title, imageUrl);
        return CreatedAtAction(nameof(GetSignature), new { id = created.Id }, created);
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult> BulkCreateSignatures([FromForm] string title, [FromForm] string namesJson, [FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { message = "En az bir dosya gereklidir." });

        var names = JsonSerializer.Deserialize<List<string>>(namesJson) ?? [];
        if (names.Count != files.Count)
            return BadRequest(new { message = "Dosya ve isim sayisi eslesmiyor." });

        var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "signatures");
        Directory.CreateDirectory(uploadsDir);

        var allowedExts = new[] { ".png", ".jpg", ".jpeg" };
        var items = new List<(string name, string title, string imageUrl)>();

        for (int i = 0; i < files.Count; i++)
        {
            var file = files[i];
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExts.Contains(ext))
                continue;

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            items.Add((names[i], title, $"/uploads/signatures/{fileName}"));
        }

        if (items.Count == 0)
            return BadRequest(new { message = "Gecerli dosya bulunamadi. Sadece PNG ve JPG kabul edilir." });

        var created = await _crud.BulkCreateSignaturesAsync(items);
        return Ok(new { count = created.Count });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateSignature(int id, [FromForm] string name, [FromForm] string title, [FromForm] IFormFile? file)
    {
        var signature = await _crud.GetSignatureAsync(id);
        if (signature == null) return NotFound();

        signature.Name = name;
        signature.Title = title;

        if (file != null && file.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
                return BadRequest(new { message = "Sadece PNG ve JPG dosyalari kabul edilir." });

            var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "signatures");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            signature.ImageUrl = $"/uploads/signatures/{fileName}";
        }

        await _crud.UpdateSignatureAsync(signature);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSignature(int id)
    {
        var found = await _crud.DeleteSignatureAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }
}
