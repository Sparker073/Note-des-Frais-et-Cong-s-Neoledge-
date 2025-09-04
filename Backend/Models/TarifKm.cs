using System;
using System.Collections.Generic;

namespace MonBackend.Models;

public class TarifKm
{
    public int Id { get; set; }

    public string CategorieVehicule { get; set; } // ex: "Voiture", "Moto"
    public decimal TarifParKm { get; set; }
}