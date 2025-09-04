using Microsoft.AspNetCore.Mvc;
using MonBackend.Models;
using MonBackend.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;



namespace MonBackend.Controllers
{
    
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LigneNoteFraisController : ControllerBase
    {
        private readonly ILigneNoteFraisService _service;

        public LigneNoteFraisController(ILigneNoteFraisService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var lignes = await _service.GetAllAsync();
                return Ok(lignes);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Erreur interne.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var ligne = await _service.GetByIdAsync(id);
                var note = ligne.NoteDeFrais;
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (note.UserId != userId && currentUserRole != "Admin")
                {
                    return Forbid(); // 403 si non autorisé
                }

                return Ok(ligne);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Erreur interne.");
            }
        }

        [HttpGet("note/{noteId}")]
        public async Task<IActionResult> GetByNoteDeFraisId(int noteId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var lignes = await _service.GetByNoteDeFraisIdAsync(noteId);
                var note = lignes.FirstOrDefault().NoteDeFrais;

                if (note.UserId != userId && currentUserRole != "Admin")
                {
                    return Forbid(); // 403 si non autorisé
                }

                return Ok(lignes);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Erreur interne.");
            }
        }

        // userId passé en paramètre, adapte selon ta manière de récupérer l'utilisateur (token, session, etc.)
        [HttpPost()]
        public async Task<IActionResult> Create([FromBody] LigneNoteFrais ligne)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var created = await _service.CreateAsync(ligne, userId);
                var note = created.NoteDeFrais;

                if (note.UserId != userId && currentUserRole != "Admin")
                {
                    return Forbid(); // 403 si non autorisé
                }
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Erreur interne.");
            }
        }

        [HttpPut()]
        public async Task<IActionResult> Update([FromBody] LigneNoteFrais ligne)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                // ✅ FIRST: Check if the line exists and get the associated note
                var existingLigne = await _service.GetByIdAsync(ligne.Id);
                var note = existingLigne.NoteDeFrais;

                // ✅ SECOND: Check authorization BEFORE attempting update
                if (note.UserId != userId && currentUserRole != "Admin")
                {
                    return Forbid("You are not authorized to update this expense line.");
                }

                // ✅ THIRD: Now attempt the update
                var updated = await _service.UpdateAsync(ligne, userId);
                
                return Ok(updated);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Add some logging here to see what's really happening
                Console.WriteLine($"Unexpected error updating ligne {ligne.Id}: {ex.Message}");
                return StatusCode(500, "Erreur interne.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var result = await _service.DeleteAsync(id, userId);
                var note = result.NoteDeFrais;
                
                if (note.UserId != userId && currentUserRole != "Admin")
                {
                    return Forbid(); // 403 si non autorisé
                }
                if (result != null)
                    return NoContent();
                return BadRequest("Impossible de supprimer la ligne.");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Erreur interne.");
            }
        }
    }
}
