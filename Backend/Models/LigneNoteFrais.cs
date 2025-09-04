using System;
using System.Collections.Generic;

namespace MonBackend.Models;

public class LigneNoteFrais
{
    public int Id { get; set; }

    public int NoteDeFraisId { get; set; }
    public NoteDeFrais? NoteDeFrais { get; set; }

    public DateTime Date { get; set; }
    public string Description { get; set; }
    public decimal Montant { get; set; }

    // champ pour la preuve de la note de frais
    public string? JustificatifPath { get; set; }

    // Facultatif pour un déplacement
    public int? TarifKmId { get; set; }
    //TarifKm est un objet qui contient le tarif par km pour un type de véhicule
    public TarifKm? TarifKm { get; set; }

    public int? DistanceKm { get; set; } // Si c’est un trajet
}
