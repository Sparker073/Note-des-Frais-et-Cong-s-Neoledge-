using System;
using System.ComponentModel.DataAnnotations;

namespace MonBackend.DTOs;

public class UpdateJourFerieDto
{
    public DateTime? Date { get; set; }
    
    [StringLength(200, ErrorMessage = "La description ne peut pas dépasser 200 caractères")]
    public string? Description { get; set; }
}