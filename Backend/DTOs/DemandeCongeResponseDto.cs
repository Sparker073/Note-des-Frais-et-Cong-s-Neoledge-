using System;
using System.ComponentModel.DataAnnotations;
using MonBackend.Models;

namespace MonBackend.DTOs;

public class DemandeCongeResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string NomEmploye { get; set; }
    public string EmailEmploye { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public TypeCongé Type { get; set; }
    public StatutDemande Statut { get; set; }
    public DateTime DateDemande { get; set; }
    public string? Commentaire { get; set; }
    public string? CommentaireManager { get; set; }
    public int NombreJours { get; set; }
}