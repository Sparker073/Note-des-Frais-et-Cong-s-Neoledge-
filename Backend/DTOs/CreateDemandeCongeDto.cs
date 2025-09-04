using System;
using System.ComponentModel.DataAnnotations;
using MonBackend.Models;

namespace MonBackend.DTOs;

public class CreateDemandeCongeDto
{
    [Required(ErrorMessage = "La date de début est requise")]
    public DateTime DateDebut { get; set; }

    [Required(ErrorMessage = "La date de fin est requise")]
    public DateTime DateFin { get; set; }

    [Required(ErrorMessage = "Le type de congé est requis")]
    public TypeCongé Type { get; set; }

    public string? Commentaire { get; set; }
}