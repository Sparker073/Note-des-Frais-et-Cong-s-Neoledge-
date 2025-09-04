using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace MonBackend.Models;

public class JourFerie
{   
    //la date d'un jour ferié est consideré toujours comme un seul jour
    public int Id { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Description { get; set; }
}