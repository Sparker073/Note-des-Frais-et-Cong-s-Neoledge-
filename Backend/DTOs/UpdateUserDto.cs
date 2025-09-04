using System.ComponentModel.DataAnnotations;
using MonBackend.Models;

namespace MonBackend.DTOs;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Le nom est requis")]
    public string Nom { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Le rôle est requis")]
    public string Role { get; set; } = "Employe";

    public string Poste { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
}