using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonBackend.Models;
using MonBackend.Repositories.Interfaces;
using MonBackend.Services.Interfaces;
using System.Linq;

namespace MonBackend.Services
{
    public class LigneNoteFraisService : ILigneNoteFraisService
    {
        private readonly ILigneNoteFraisRepository _repository;
        private readonly INoteDeFraisRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITarifKmRepository _tarifKmRepository;

        public LigneNoteFraisService(
            ILigneNoteFraisRepository repository,
            INoteDeFraisRepository noteRepository,
            IUserRepository userRepository,
            ITarifKmRepository tarifKmRepository)
        {
            _repository = repository;
            _noteRepository = noteRepository;
            _userRepository = userRepository;
            _tarifKmRepository = tarifKmRepository;
        }

        public async Task<IEnumerable<LigneNoteFrais>> GetAllAsync()
        {
            var lignes = await _repository.GetAllAsync();
            if (!lignes.Any())
                throw new ArgumentException("Aucune ligne de note trouvée !");
            return lignes;
        }

        public async Task<LigneNoteFrais?> GetByIdAsync(int id)
        {
            var exist = await _repository.GetByIdAsync(id);
            if (exist == null)
                throw new KeyNotFoundException($"Ligne de Note avec Id={id} non trouvée.");
            return exist;
        }

        public async Task<IEnumerable<LigneNoteFrais>> GetByNoteDeFraisIdAsync(int noteId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId);
            if (note == null)
                throw new KeyNotFoundException($"Note de frais introuvable avec l'Id={noteId}.");

            var lignes = await _repository.GetByNoteDeFraisIdAsync(noteId);
            if (!lignes.Any())
                throw new ArgumentException($"Aucune ligne trouvée pour la note avec Id={noteId}.");
            return lignes;
        }

        // Ajout de userId en paramètre pour vérifier l'autorisation
        public async Task<LigneNoteFrais> CreateAsync(LigneNoteFrais ligne, int userId)
        {
            if (ligne == null)
                throw new ArgumentNullException(nameof(ligne));

            var note = await _noteRepository.GetByIdAsync(ligne.NoteDeFraisId);
            if (note == null)
                throw new KeyNotFoundException("Note de frais introuvable pour l'association.");

            if (note.UserId != userId)
                throw new UnauthorizedAccessException("Vous ne pouvez pas ajouter une ligne à une note qui ne vous appartient pas.");

            if (ligne.Date > DateTime.Now)
                throw new ArgumentException("La date de la ligne de frais ne peut pas être dans le futur.");

            if (string.IsNullOrWhiteSpace(ligne.Description))
                throw new ArgumentException("La description est obligatoire.");

            if (ligne.Montant < 0) 
                throw new ArgumentException("Le montant doit être positif ou nul.");

            if (ligne.TarifKmId.HasValue)
            {
                if (ligne.DistanceKm == null || ligne.DistanceKm < 0)
                    throw new ArgumentException("La distance kilométrique doit être renseignée et positive.");

                var tarifKm = await _tarifKmRepository.GetByIdAsync(ligne.TarifKmId.Value);
                if (tarifKm == null)
                    throw new KeyNotFoundException("Le tarif kilométrique spécifié est introuvable.");
            }

            // **Check for duplicate before creating**
            var existingLignes = await _repository.GetByNoteDeFraisIdAsync(ligne.NoteDeFraisId);
            bool duplicateExists = existingLignes.Any(l =>
                l.Date == ligne.Date &&
                l.Description.Trim().Equals(ligne.Description.Trim(), StringComparison.OrdinalIgnoreCase) &&
                l.Montant == ligne.Montant &&
                l.TarifKmId == ligne.TarifKmId &&
                l.DistanceKm == ligne.DistanceKm
            );

            if (duplicateExists)
                throw new ArgumentException("Une ligne identique existe déjà dans cette note de frais.");

            return await _repository.CreateAsync(ligne);
        }



       public async Task<LigneNoteFrais?> UpdateAsync(LigneNoteFrais ligne, int userId)
        {
            if (ligne == null)
                throw new ArgumentNullException(nameof(ligne));

            var existing = await _repository.GetByIdAsync(ligne.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Ligne de note avec Id={ligne.Id} introuvable.");

            var note = await _noteRepository.GetByIdAsync(existing.NoteDeFraisId);
            if (note == null)
                throw new KeyNotFoundException("Note de frais associée introuvable.");

            if (note.UserId != userId)
                throw new UnauthorizedAccessException("Vous n'avez pas le droit de modifier une ligne appartenant à une note d'un autre utilisateur.");

            if (ligne.NoteDeFraisId != note.Id)
                throw new ArgumentException("Incohérence : la ligne ne correspond pas à sa note originale.");

            if (ligne.Date > DateTime.Now)
                throw new ArgumentException("La date de la ligne de frais ne peut pas être dans le futur.");

            if (string.IsNullOrWhiteSpace(ligne.Description))
                throw new ArgumentException("La description est obligatoire.");

            if (ligne.Montant < 0)
                throw new ArgumentException("Le montant doit être positif ou nul.");

            if (ligne.TarifKmId.HasValue)
            {
                if (ligne.DistanceKm == null || ligne.DistanceKm < 0)
                    throw new ArgumentException("La distance kilométrique doit être renseignée et positive.");

                var tarifKm = await _tarifKmRepository.GetByIdAsync(ligne.TarifKmId.Value);
                if (tarifKm == null)
                    throw new KeyNotFoundException("Le tarif kilométrique spécifié est introuvable.");
            }

            // Optionnel : gestion justificatif etc.

            return await _repository.UpdateAsync(ligne);
        }


        public async Task<LigneNoteFrais> DeleteAsync(int id, int userId)
        {
            var ligne = await _repository.GetByIdAsync(id);
            if (ligne == null)
                throw new KeyNotFoundException($"Ligne avec Id={id} introuvable.");

            var note = await _noteRepository.GetByIdAsync(ligne.NoteDeFraisId);
            if (note == null)
                throw new KeyNotFoundException("Note de frais associée introuvable.");

            if (note.UserId != userId)
                throw new UnauthorizedAccessException("Vous ne pouvez pas supprimer une ligne appartenant à une note d'un autre utilisateur.");

            return await _repository.DeleteAsync(id);
        }
    }
}
