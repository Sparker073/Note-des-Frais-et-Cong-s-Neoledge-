using Microsoft.AspNetCore.Mvc;
using MonBackend.Services;
using MonBackend.Models;
using MonBackend.Common;
using Microsoft.AspNetCore.Authorization;


namespace MonBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TarifKmController : ControllerBase
{
    private readonly ITarifKmService _service;

    public TarifKmController(ITarifKmService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Ok(new
        {
            message = "Liste des tarifs récupérée avec succès.",
            data = list
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        try
        {
            var tarif = await _service.GetByIdAsync(id);
            return Ok(new
            {
                message = "Tarif trouvé avec succès.",
                data = tarif
            });
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(knf.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] TarifKm tarif)
    {
        try
        {
            var created = await _service.CreateAsync(tarif);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
            {
                message = "Tarif créé avec succès.",
                data = created
            });
        }
        catch (ArgumentException argEx)
        {
            return BadRequest(argEx.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, TarifKm tarif)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, tarif);
            return Ok(new
            {
                message = "Tarif mis à jour avec succès.",
                data = updated
            });
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(knf.Message);
        }
        catch (ArgumentException argEx)
        {
            return BadRequest(argEx.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(new
            {
                message = "Tarif supprimé avec succès."
            });
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(knf.Message);
        }
    }

    [HttpGet("by-categorie/{categorie}")]
    public async Task<ActionResult> GetByCategorie(string categorie)
    {
        var tarif = await _service.GetByCategorieAsync(categorie);
        if (tarif == null)
            return NotFound($"Aucun tarif trouvé pour la catégorie {categorie}");

        return Ok(new
        {
            message = "Tarif trouvé pour la catégorie.",
            data = tarif
        });
    }
}
