        using System;
        using System.Collections.Generic;
        using MonBackend.Models;
        using LigneNoteFrais = MonBackend.Models.LigneNoteFrais;
        using Projet = MonBackend.Models.Projet;
        using User = MonBackend.Models.User;

        namespace MonBackend.Models;

        public enum StatutNoteFrais
        {
            EnAttente,
            Approuvee,
            Refusee
        }

        public class NoteDeFrais
        {
            public int Id { get; set; }

            public int UserId { get; set; }
            public User? Employe { get; set; }

            public DateTime DateSoumission { get; set; } = DateTime.Now;

            public StatutNoteFrais Statut { get; set; } = StatutNoteFrais.EnAttente;

            public ICollection<LigneNoteFrais>? Lignes { get; set; } = new List<LigneNoteFrais>();

            public int? ProjetId { get; set; }
            public Projet? Projet { get; set; }

            public string? commentaireMananger { get; set; }

            
        }
