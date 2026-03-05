using Sertifika.Entities;
using Sertifika.Factories.Holders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sertifika.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HoldersController : ControllerBase
{
    private readonly IHolderCrudFactory _crud;

    public HoldersController(IHolderCrudFactory crud)
    {
        _crud = crud;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Holder>>> GetHolders()
    {
        return Ok(await _crud.GetHoldersAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Holder>> GetHolder(int id)
    {
        var holder = await _crud.GetHolderAsync(id);
        if (holder == null) return NotFound();
        return holder;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<ActionResult<Holder>> CreateHolder(Holder holder)
    {
        var created = await _crud.CreateHolderAsync(holder);
        return CreatedAtAction(nameof(GetHolder), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,CertificateCreator")]
    public async Task<IActionResult> UpdateHolder(int id, Holder holder)
    {
        if (id != holder.Id) return BadRequest();
        await _crud.UpdateHolderAsync(holder);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteHolder(int id)
    {
        var found = await _crud.DeleteHolderAsync(id);
        if (!found) return NotFound();
        return NoContent();
    }
}
