using System;
using System.ComponentModel.DataAnnotations;
using MonBackend.Models;

namespace MonBackend.DTOs;

public class UpdateStatutDemandeDto
{
    [Required(ErrorMessage = "Le statut est requis")]
    public StatutDemande Statut { get; set; }

    public string? CommentaireManager { get; set; }
}   