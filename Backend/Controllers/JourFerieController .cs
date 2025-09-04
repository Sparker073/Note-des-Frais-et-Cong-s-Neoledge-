using Microsoft.AspNetCore.Mvc;
using MonBackend.Services.Interfaces;
using MonBackend.DTOs;
using MonBackend.Models;
using MonBackend.Common;
using Microsoft.AspNetCore.Authorization;


namespace MonBackend.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class JourFerieController : ControllerBase
{
    private readonly IJourFerieService _jourFerieService;

    public JourFerieController(IJourFerieService jourFerieService)
    {
        _jourFerieService = jourFerieService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JourFerie>>>> GetAllJoursFeries()
    {
        try
        {
            var joursFeries = await _jourFerieService.GetAllJoursFeriesAsync();
            return Ok(ApiResponse<List<JourFerie>>.SuccessResponse(joursFeries, "Jours fériés récupérés avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<JourFerie>>.ErrorResponse("Erreur lors de la récupération des jours fériés"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<JourFerie>>> GetJourFerieById(int id)
    {
        try
        {
            var jourFerie = await _jourFerieService.GetJourFerieByIdAsync(id);
            if (jourFerie == null)
                return NotFound(ApiResponse<JourFerie>.ErrorResponse("Jour férié introuvable"));

            return Ok(ApiResponse<JourFerie>.SuccessResponse(jourFerie, "Jour férié récupéré avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<JourFerie>.ErrorResponse("Erreur lors de la récupération du jour férié"));
        }
    }

    [HttpGet("year/{year}")]
    public async Task<ActionResult<ApiResponse<List<JourFerie>>>> GetJoursFeriesByYear(int year)
    {
        try
        {
            var joursFeries = await _jourFerieService.GetJoursFeriesByYearAsync(year);
            return Ok(ApiResponse<List<JourFerie>>.SuccessResponse(joursFeries, "Jours fériés de l'année récupérés avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<JourFerie>>.ErrorResponse("Erreur lors de la récupération des jours fériés"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<JourFerie>>> CreateJourFerie([FromBody] CreateJourFerieDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<JourFerie>.ErrorResponse("Données invalides", errors));
            }

            var jourFerie = await _jourFerieService.CreateJourFerieAsync(createDto);
            return CreatedAtAction(nameof(GetJourFerieById), new { id = jourFerie.Id },
                ApiResponse<JourFerie>.SuccessResponse(jourFerie, "Jour férié créé avec succès"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<JourFerie>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<JourFerie>.ErrorResponse("Erreur lors de la création du jour férié"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<JourFerie>>> UpdateJourFerie(int id, [FromBody] UpdateJourFerieDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<JourFerie>.ErrorResponse("Données invalides", errors));
            }

            var jourFerie = await _jourFerieService.UpdateJourFerieAsync(id, updateDto);
            if (jourFerie == null)
                return NotFound(ApiResponse<JourFerie>.ErrorResponse("Jour férié introuvable"));

            return Ok(ApiResponse<JourFerie>.SuccessResponse(jourFerie, "Jour férié mis à jour avec succès"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<JourFerie>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<JourFerie>.ErrorResponse("Erreur lors de la mise à jour du jour férié"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteJourFerie(int id)
    {
        try
        {
            var result = await _jourFerieService.DeleteJourFerieAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Jour férié introuvable"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Jour férié supprimé avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Erreur lors de la suppression du jour férié"));
        }
    }

    [HttpGet("check")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckJourFerie([FromQuery] DateTime date)
    {
        try
        {
            var isJourFerie = await _jourFerieService.IsJourFerieAsync(date);
            string message = isJourFerie ? "Cette date est un jour férié" : "Cette date n'est pas un jour férié";
            return Ok(ApiResponse<bool>.SuccessResponse(isJourFerie, message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Erreur lors de la vérification du jour férié"));
        }
    }
}