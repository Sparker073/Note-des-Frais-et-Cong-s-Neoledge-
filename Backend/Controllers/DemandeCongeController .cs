using Microsoft.AspNetCore.Mvc;
using MonBackend.Services.Interfaces;
using MonBackend.Repositories.Interfaces;
using MonBackend.DTOs;
using MonBackend.Models;
using MonBackend.Common;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MonBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DemandeCongeController : ControllerBase
{

    //TO DO : Escalade : en cas d’absence du manager, possibilité qu’un autre valide (supérieur hiérarchique).

    private readonly IDemandeCongeService _demandeCongeService;
    private readonly IUserService _userService;
    private readonly IDemandeCongeRepository _demandeRepository;

    public DemandeCongeController(IDemandeCongeService demandeCongeService,IUserService userService,IDemandeCongeRepository demandeRepository)
    {
        _demandeCongeService = demandeCongeService;
        _userService = userService;
        _demandeRepository = demandeRepository;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DemandeCongeResponseDto>>>> GetAllDemandes()
    {
        try
        {
            var demandes = await _demandeCongeService.GetAllDemandesAsync();

            if (demandes == null || !demandes.Any())
            {
                return NotFound(ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Aucune demande trouvée !"));
            }

            return Ok(ApiResponse<List<DemandeCongeResponseDto>>.SuccessResponse(demandes, "Demandes de congés récupérées avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Erreur lors de la récupération des demandes de congés"));
        }
    }


    // [HttpGet("{id}")]
    // public async Task<ActionResult<ApiResponse<DemandeCongeResponseDto>>> GetDemandeById(int id)
    // {
    //     try
    //     {
    //         var demande = await _demandeCongeService.GetDemandeByIdAsync(id);
    //         if (demande == null)
    //             return NotFound(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Demande de congé introuvable"));

    //         return Ok(ApiResponse<DemandeCongeResponseDto>.SuccessResponse(demande, "Demande de congé récupérée avec succès"));
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Erreur lors de la récupération de la demande de congé"));
    //     }
    // }


    [HttpGet("user")]
    public async Task<ActionResult<ApiResponse<List<DemandeCongeResponseDto>>>> GetDemandesByUserIdFromToken()
    {
        try
        {
            // Récupérer l'ID utilisateur depuis les claims du token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Utilisateur non authentifié"));

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("ID utilisateur invalide dans le token"));

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Utilisateur introuvable"));

            var demandes = await _demandeCongeService.GetDemandesByUserIdAsync(userId);
            return Ok(ApiResponse<List<DemandeCongeResponseDto>>.SuccessResponse(demandes, "Demandes de congés récupérées avec succès"));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Erreur lors de la récupération des demandes de congés"));
        }
    }


    [HttpPost("user")]
    public async Task<ActionResult<ApiResponse<DemandeCongeResponseDto>>> CreateDemande([FromBody] CreateDemandeCongeDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Données invalides", errors));
            }

            // Récupérer l'ID utilisateur depuis le token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Utilisateur non authentifié"));

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("ID utilisateur invalide dans le token"));

            var (demande, joursFeries) = await _demandeCongeService.CreateDemandeAsync(userId, createDto);
            
            string message = "Votre demande a été enregistrée.";
            if (joursFeries.Any())
            {
                message += $" Attention : les dates suivantes sont des jours fériés : {string.Join(", ", joursFeries)}.";
            }

            return Ok(ApiResponse<DemandeCongeResponseDto>.SuccessResponse(demande, message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DemandeCongeResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Erreur lors de la création de la demande de congé"));
        }
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDemande(int id)
    {
        try
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized(ApiResponse<bool>.ErrorResponse("Utilisateur non identifié"));

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var demande = await _demandeRepository.GetByIdAsync(id);
            if (demande == null)
                return NotFound(ApiResponse<bool>.ErrorResponse("Demande non trouvée"));

            // Vérifie que c’est l’utilisateur propriétaire OU un admin
            if (demande.UserId != userId && userRole != "admin")
                return Forbid();

            var result = await _demandeCongeService.DeleteDemandeAsync(id);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Demande de congé supprimée avec succès"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Erreur lors de la suppression de la demande de congé"));
        }
    }


     [HttpGet("manager")]
    public async Task<ActionResult<ApiResponse<List<DemandeCongeResponseDto>>>> GetDemandesByManagerId()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int managerId))
                return Unauthorized(ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Utilisateur non identifié"));
            
            var user = await _userService.GetUserByIdAsync(managerId);
            if (user == null)
                return  NotFound(ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Manager Introuvable !"));

            var demandes = await _demandeCongeService.GetDemandesByManagerIdAsync(managerId);
            return Ok(ApiResponse<List<DemandeCongeResponseDto>>.SuccessResponse(demandes, "Demandes de congés de l'équipe récupérées avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Erreur lors de la récupération des demandes de congés"));
        }
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("statut/{statut}")]
    public async Task<ActionResult<ApiResponse<List<DemandeCongeResponseDto>>>> GetDemandesByStatut(StatutDemande statut)
    {
        try
        {
            var demandes = await _demandeCongeService.GetDemandesByStatutAsync(statut);
            if (demandes == null)
                return  NotFound(ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Aucune Demande avec ce status !"));
            return Ok(ApiResponse<List<DemandeCongeResponseDto>>.SuccessResponse(demandes, "Demandes de congés par statut récupérées avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<DemandeCongeResponseDto>>.ErrorResponse("Erreur lors de la récupération des demandes de congés"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DemandeCongeResponseDto>>> UpdateDemande(int id, [FromBody] UpdateDemandeCongeDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Données invalides", errors));
            }
            
            
            var existdemande = await _demandeCongeService.GetDemandeByIdAsync(id);
            if (existdemande == null)
                return NotFound(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Demande non trouvée"));
            
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (existdemande.UserId != currentUserId && currentUserRole != "Admin")
            {
                return Forbid(); // 403 si non autorisé
            }

            var (demande,joursFeries) = await _demandeCongeService.UpdateDemandeAsync(id, updateDto);
            string message = "Votre demande est mise a jour  avec succes .";
            if (joursFeries.Any())
            {
                message += $" Attention : les dates suivantes sont des jours fériés : {string.Join(", ", joursFeries)}.";
            }

            return Ok(ApiResponse<DemandeCongeResponseDto>.SuccessResponse(demande,message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DemandeCongeResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Erreur lors de la mise à jour de la demande de congé"));
        }
    }
 

    [HttpPatch("{id}/statut")]
    public async Task<ActionResult<ApiResponse<DemandeCongeResponseDto>>> UpdateStatutDemande(int id, [FromBody] UpdateStatutDemandeDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Données invalides", errors));
            }

            var managerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var exist_demande = await _demandeRepository.GetByIdAsync(id);
            if (exist_demande == null)
                return NotFound(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Demande de congé introuvable"));
            var exist_manager = await _userService.GetUserByIdAsync(managerId);
            var user = await _userService.GetUserByIdAsync(exist_demande.UserId);
            if (exist_manager == null)
                 return NotFound(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Manager Introuvable !"));
            if (user.ManagerId != managerId && currentUserRole != "Admin")
                 return NotFound(ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Vous n'avez pas l'autorisation de modifier cette demande"));


            var demande = await _demandeCongeService.UpdateStatutDemandeAsync(id, managerId, updateDto);            

            string message = updateDto.Statut == StatutDemande.Approuve ? "Demande de congé approuvée" : "Demande de congé refusée";
            return Ok(ApiResponse<DemandeCongeResponseDto>.SuccessResponse(demande, message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DemandeCongeResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DemandeCongeResponseDto>.ErrorResponse("Erreur lors de la mise à jour du statut"));
        }
    }


    [HttpGet("solde/{year}")]
    public async Task<ActionResult<ApiResponse<int>>> GetSoldeConges(int year)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var solde = await _demandeCongeService.GetSoldeCongesAsync(userId, year);
            return Ok(ApiResponse<int>.SuccessResponse(solde, "Solde de congés récupéré avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Erreur lors de la récupération du solde de congés"));
        }
    }


}