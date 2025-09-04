using Microsoft.AspNetCore.Mvc;
using MonBackend.Models;
using MonBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MonBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NoteDeFraisController : ControllerBase
    {
        private readonly INoteDeFraisService _service;

        public NoteDeFraisController(INoteDeFraisService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteDeFrais>>> GetAll()
        {
            try{
                var notes = await _service.GetAllAsync();
                return Ok(notes);
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteDeFrais>> GetById(int id)
        {
            try
            {
                var note = await _service.GetByIdAsync(id);
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (note.UserId != userId && currentUserRole != "Admin")
                {
                    return Forbid(); // 403 si non autorisé
                }

                return Ok(note);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<NoteDeFrais>>> GetByUserId()
        {
            try
            {
                
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                IEnumerable<NoteDeFrais> notes = null;
                if (currentUserRole == "Admin")
                    notes = await _service.GetAllAsync();
                else
                    {
                        notes = await _service.GetByUserIdAsync(userId);
                    }
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("manager")]
        public async Task<ActionResult<IEnumerable<NoteDeFrais>>> GetByManagerId()
        {
            try
            {
                var managerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var notes = await _service.GetByManagerIdAsync(managerId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet("projet/{projetId}")]
        public async Task<ActionResult<IEnumerable<NoteDeFrais>>> GetByProjetId(int projetId)
        {
            try
            {
                var notes = await _service.GetByProjetIdAsync(projetId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<NoteDeFrais>> Create([FromBody] NoteDeFrais note)
        {
            try
            {
                var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Si l'utilisateur n'est pas admin, forcer le userId à celui du token
                if (userRole != "Admin")
                {
                    note.UserId = userIdFromToken;
                }
                else
                {
                    // Si admin, s'assurer qu'un UserId est bien fourni
                    if (note.UserId == null)
                        return BadRequest("L'ID de l'utilisateur est requis pour un admin.");
                }

                var created = await _service.CreateAsync(note);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne : {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<NoteDeFrais>> Update(int id, [FromBody] NoteDeFrais updatedNote)
        {
            try
            {
                var userIdFromToken = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var existingNote = await _service.GetByIdAsync(id);
                if (existingNote == null)
                    return NotFound("Note de frais introuvable.");

                // Autorisation : admin OU propriétaire de la note
                if (userRole != "Admin" && existingNote.UserId != userIdFromToken)
                    return Forbid("Vous n'avez pas l'autorisation de modifier cette note.");

                // On garde l'UserId original (non modifiable) même si un utilisateur essaie de le modifier
                updatedNote.UserId = existingNote.UserId;

                var updated = await _service.UpdateAsync(id, updatedNote);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne : {ex.Message}");
            }
        }


        //Admin ne peut pas changer le status dune note dans ce cas il faut la changer 
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] NoteDeFrais note)
        {
            try
            {
                // ADD DEBUGGING TO SEE WHAT YOU'RE RECEIVING
                Console.WriteLine($"🚨 DEBUG: Received note ID: {note.Id}");
                Console.WriteLine($"🚨 DEBUG: Received status: {note.Statut}");
                Console.WriteLine($"🚨 DEBUG: Received comment: '{note.commentaireMananger}'");
                Console.WriteLine($"🚨 DEBUG: Comment is null: {note.commentaireMananger == null}");
                Console.WriteLine($"🚨 DEBUG: Comment is empty: {string.IsNullOrEmpty(note.commentaireMananger)}");
                
                var managerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                Console.WriteLine($"🚨 DEBUG: Manager ID: {managerId}");
                
                var updatedNote = await _service.UpdateStatus(managerId, note);
                
                    Console.WriteLine($"🚨 DEBUG: Updated note comment: '{updatedNote.commentaireMananger}'");
                
                return Ok(updatedNote);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"🚨 DEBUG: ArgumentException: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"🚨 DEBUG: KeyNotFoundException: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"🚨 DEBUG: InvalidOperationException: {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🚨 DEBUG: Exception: {ex.Message}");
                Console.WriteLine($"🚨 DEBUG: Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Une erreur interne est survenue.");
            }
        }

        //exemple json pour update status {
        //   "id":3,
        //   "userId":1,
        //   "statut": "Approuvee"}

        

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var note = await _service.GetByIdAsync(id); // suppose que cette méthode existe

                if (note == null)
                    return NotFound("Note de frais introuvable.");

                 if (note.UserId != userId && userRole != "Admin")
                {
                    return Forbid(); // 403 si non autorisé
                }


                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne : {ex.Message}");
            }
        }

    }
}
