using System;
using System.ComponentModel.DataAnnotations;
using MonBackend.Models;

namespace MonBackend.DTOs;

public class UpdateDemandeCongeDto
{
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public TypeCongé? Type { get; set; }
    public string? Commentaire { get; set; }
}