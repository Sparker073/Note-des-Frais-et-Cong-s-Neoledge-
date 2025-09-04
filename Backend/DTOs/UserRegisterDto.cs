using System;
using System.ComponentModel.DataAnnotations;
using MonBackend.Models;

namespace MonBackend.DTOs;

public class UserRegisterDto
{
    public string Nom { get; set; }
    public string Email { get; set; }
    public string motDePasse { get; set; }
    public string Poste { get; set; }
    public int? ManagerId { get; set; }
}