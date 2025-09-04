using System;
using System.Collections.Generic;
namespace MonBackend.Models;
using System.ComponentModel.DataAnnotations;


public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nom { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; }

    [Required]
    public string MotDePasse { get; set; }

    [Required]
    public string Role { get; set; }

    [Required]
    public string Poste { get; set; }
    
    public int? ManagerId { get; set; }
    public User? Manager { get; set; }
    
    public List<User> Subordonnes { get; set; } = new();
    public List<DemandeCongé> DemandesConge { get; set; } = new();
    
    // Solde de congés par défaut 
    public int SoldeCongesAnnuel { get; set; } = 30;
}