using System;
using System.Collections.Generic;

namespace MonBackend.Models;

public class Projet
{
    public int Id { get; set; }

    public string Nom { get; set; }
    public string Description { get; set; }

    public ICollection<NoteDeFrais>? NotesDeFrais { get; set; }
}