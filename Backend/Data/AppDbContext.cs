using Microsoft.EntityFrameworkCore;
using MonBackend.Models; 
using DemandeCongé = MonBackend.Models.DemandeCongé;
using TypeCongé = MonBackend.Models.TypeCongé;
using JourFerie = MonBackend.Models.JourFerie;
using LigneNoteFrais = MonBackend.Models.LigneNoteFrais;
using Projet = MonBackend.Models.Projet;
using TarifKm = MonBackend.Models.TarifKm;

namespace MonBackend.Data 
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<DemandeCongé> DemandesCongés { get; set; }
        public DbSet<JourFerie> JourFeries { get; set; }
        public DbSet<NoteDeFrais> NotesDeFrais { get; set; }
        public DbSet<LigneNoteFrais> LignesNotesFrais { get; set; }
        public DbSet<Projet> Projets { get; set; }
        public DbSet<TarifKm> TarifsKm { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la relation User - Manager
            modelBuilder.Entity<User>()
                .HasOne(u => u.Manager)
                .WithMany(u => u.Subordonnes)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration de la relation User - DemandeCongé
            modelBuilder.Entity<DemandeCongé>()
                .HasOne(d => d.Employe)
                .WithMany(u => u.DemandesConge)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration des contraintes pour DemandeCongé
            modelBuilder.Entity<DemandeCongé>()
                .HasIndex(d => new { d.UserId, d.DateDebut, d.DateFin })
                .HasDatabaseName("IX_DemandeCongé_UserDateRange");

            // Configuration des contraintes pour JourFerie
            modelBuilder.Entity<JourFerie>()
                .HasIndex(j => j.Date)
                .IsUnique()
                .HasDatabaseName("IX_JourFerie_Date_Unique");

            // Configuration des propriétés
            modelBuilder.Entity<DemandeCongé>()
                .Property(d => d.Type)
                .HasConversion<string>();

            modelBuilder.Entity<DemandeCongé>()
                .Property(d => d.Statut)
                .HasConversion<string>();

            // Configuration des valeurs par défaut
            modelBuilder.Entity<DemandeCongé>()
                .Property(d => d.DateDemande)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<DemandeCongé>()
                .Property(d => d.Statut)
                .HasDefaultValue(StatutDemande.EnAttente);

            modelBuilder.Entity<User>()
                .Property(u => u.SoldeCongesAnnuel)
                .HasDefaultValue(30);

            // Seed data pour les jours fériés (optionnel)
            SeedJoursFeries(modelBuilder);
        }

        private void SeedJoursFeries(ModelBuilder modelBuilder)
        {
            var currentYear = DateTime.Now.Year;
            
            modelBuilder.Entity<JourFerie>().HasData(
                new JourFerie { Id = 1, Date = new DateTime(currentYear, 1, 1), Description = "Jour de l'An" },
                new JourFerie { Id = 2, Date = new DateTime(currentYear, 3, 20), Description = "Fête de l'Indépendance" },
                new JourFerie { Id = 3, Date = new DateTime(currentYear, 4, 9), Description = "Fête des Martyrs" },
                new JourFerie { Id = 4, Date = new DateTime(currentYear, 5, 1), Description = "Fête du Travail" },
                new JourFerie { Id = 5, Date = new DateTime(currentYear, 7, 25), Description = "Fête de la République" },
                new JourFerie { Id = 6, Date = new DateTime(currentYear, 8, 13), Description = "Fête de la Femme" },
                new JourFerie { Id = 7, Date = new DateTime(currentYear, 10, 15), Description = "Fête de l'Évacuation" }
            );
        }
    }
}
