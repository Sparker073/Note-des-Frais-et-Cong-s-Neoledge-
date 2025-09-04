using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MonBackend.Models;

public enum StatutDemande
{
    EnAttente,
    Approuve,
    Refuse
}

public enum TypeCongé
{
    CongeAnnuel,
    Maladie,
    Maternite,
    Paternite,
    DecesProche
}

public class DemandeCongé
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User Employe { get; set; }

    [Required]
    public DateTime DateDebut { get; set; }
    
    [Required]
    public DateTime DateFin { get; set; }

    [Required]
    public TypeCongé Type { get; set; }

    public StatutDemande Statut { get; set; } = StatutDemande.EnAttente;

    public DateTime DateDemande { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string? Commentaire { get; set; }

    // Ajout optionnel pour les commentaires du manager
    [MaxLength(500)]
    public string? CommentaireManager { get; set; }
}