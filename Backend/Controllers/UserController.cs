using Microsoft.AspNetCore.Mvc;
using MonBackend.Services.Interfaces;
using MonBackend.DTOs;
using MonBackend.Models;
using MonBackend.Common;
using Microsoft.AspNetCore.Authorization;


namespace MonBackend.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{   
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<User>>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<List<User>>.SuccessResponse(users, "Utilisateurs récupérés avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<User>>.ErrorResponse("Erreur lors de la récupération des utilisateurs"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<User>>> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse<User>.ErrorResponse("Utilisateur introuvable"));
            
            return Ok(ApiResponse<User>.SuccessResponse(user, "Utilisateur récupéré avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<User>.ErrorResponse("Erreur lors de la récupération de l'utilisateur"));
        }
    }

    [HttpGet("by-email")]
    public async Task<ActionResult<ApiResponse<User>>> GetUserByEmail([FromQuery] string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound(ApiResponse<User>.ErrorResponse("Utilisateur introuvable"));
            
            return Ok(ApiResponse<User>.SuccessResponse(user, "Utilisateur récupéré avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<User>.ErrorResponse("Erreur lors de la récupération de l'utilisateur"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<User>>> CreateUser([FromBody] CreateUserDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<User>.ErrorResponse("Données invalides", errors));
            }

            var user = await _userService.CreateUserAsync(createDto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, 
                ApiResponse<User>.SuccessResponse(user, "Utilisateur créé avec succès"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<User>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<User>.ErrorResponse("Erreur lors de la création de l'utilisateur"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<User>>> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<User>.ErrorResponse("Données invalides", errors));
            }

            var user = await _userService.UpdateUserAsync(id, updateDto);
            if (user == null)
                return NotFound(ApiResponse<User>.ErrorResponse("Utilisateur introuvable"));

            return Ok(ApiResponse<User>.SuccessResponse(user, "Utilisateur mis à jour avec succès"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<User>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<User>.ErrorResponse("Erreur lors de la mise à jour de l'utilisateur"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResponse("Utilisateur introuvable"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Utilisateur supprimé avec succès"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Erreur lors de la suppression de l'utilisateur"));
        }
    }

    [HttpGet("{id}/subordonnes")]
    public async Task<ActionResult<ApiResponse<List<User>>>> GetSubordonnes(int id)
    {
        try
        {
            var subordonnes = await _userService.GetSubordonnesAsync(id);
            return Ok(ApiResponse<List<User>>.SuccessResponse(subordonnes, "Subordonnés récupérés avec succès"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<List<User>>.ErrorResponse(ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<User>>.ErrorResponse("Erreur lors de la récupération des subordonnés"));
        }
    }

}