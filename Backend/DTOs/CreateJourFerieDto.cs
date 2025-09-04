using System;
using System.ComponentModel.DataAnnotations;

namespace MonBackend.DTOs;

public class CreateJourFerieDto
{
    [Required(ErrorMessage = "La date est requise")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "La description est requise")]
    [StringLength(200, ErrorMessage = "La description ne peut pas dépasser 200 caractères")]
    public string Description { get; set; }
}