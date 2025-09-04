using Microsoft.AspNetCore.Mvc;
using MonBackend.Models;
using MonBackend.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace MonBackend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjetController : ControllerBase
{
    private readonly IProjetService _service;

    public ProjetController(IProjetService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Projet>>> GetAll()
    {
        var projets = await _service.GetAllAsync();
        return Ok(projets);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Projet>> GetById(int id)
    {
        try
        {
            var projet = await _service.GetByIdAsync(id);
            return Ok(projet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Projet>> Create([FromBody] Projet projet)
    {
        try
        {
            var created = await _service.CreateAsync(projet);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Projet>> Update(int id, [FromBody] Projet projet)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, projet);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

