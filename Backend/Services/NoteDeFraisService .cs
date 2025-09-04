using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonBackend.Models;
using MonBackend.Repositories.Interfaces;
using MonBackend.Services.Interfaces;

namespace MonBackend.Services
{
    public class NoteDeFraisService : INoteDeFraisService
    {
        private readonly INoteDeFraisRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IProjetRepository _projetRepository;

        public NoteDeFraisService(
            INoteDeFraisRepository repository,
            IUserRepository userRepository,
            IProjetRepository projetRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _projetRepository = projetRepository;
        }

        public async Task<IEnumerable<NoteDeFrais>> GetAllAsync()
        {
            var note = await _repository.GetAllAsync();
            if (!note.Any())
                throw new KeyNotFoundException("Aucune Note de Frais Trouvées");
            return note;
        }

        public async Task<NoteDeFrais> GetByIdAsync(int id)
        {
            var note = await _repository.GetByIdAsync(id);
            if (note == null)
                throw new KeyNotFoundException($"Note de frais avec Id={id} non trouvée.");
            return note;
        }

        public async Task<IEnumerable<NoteDeFrais>> GetByUserIdAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"Utilisateur avec Id={userId} non trouvé.");

            var notes = await _repository.GetByUserIdAsync(userId);
            if (notes == null || !notes.Any())
                throw new KeyNotFoundException($"Aucune note de frais trouvée pour l'utilisateur avec Id={userId}.");

            return notes;
        }
        public async Task<IEnumerable<NoteDeFrais>> GetByManagerIdAsync(int managerId)
        {
            var manager = await _userRepository.GetUserByIdAsync(managerId);
            if (manager == null)
                throw new KeyNotFoundException($"Manager avec Id={managerId} non trouvé.");

            var notes = await _repository.GetByManagerIdAsync(managerId);
            return notes;
        }

        public async Task<IEnumerable<NoteDeFrais>> GetByProjetIdAsync(int projetId)
        {
            var projet = await _projetRepository.GetByIdAsync(projetId);
            if (projet == null)
                throw new KeyNotFoundException($"Projet avec Id={projetId} non trouvé.");

            return await _repository.GetByProjetIdAsync(projetId);
        }

        public async Task<NoteDeFrais> CreateAsync(NoteDeFrais note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            if (note.UserId <= 0)
                throw new ArgumentException("L'utilisateur est obligatoire.");

            if (note.ProjetId == null || note.ProjetId <= 0)
                throw new ArgumentException("Le projet associé est obligatoire.");

            var user = await _userRepository.GetUserByIdAsync(note.UserId);
            if (user == null)
                throw new KeyNotFoundException($"Utilisateur avec Id={note.UserId} non trouvé.");

            var projet = await _projetRepository.GetByIdAsync(note.ProjetId.Value);
            if (projet == null)
                throw new KeyNotFoundException($"Projet avec Id={note.ProjetId} non trouvé.");

            var existNotes = await _repository.GetByUserIdAndProjetIdAsync(note.UserId, note.ProjetId.Value);
            if (existNotes!=null && existNotes.Any(n => n.Statut != StatutNoteFrais.Refusee)) 
                throw new ArgumentException("Impossible de créer deux notes pour le même projet et le même employé.");
            if (user.ManagerId == null)
                throw new KeyNotFoundException("Impossible de creer une note de frais pour un employé sans manager.");

            // TODO: Ajouter d'autres validations métiers si nécessaire

            return await _repository.CreateAsync(note);
        }

        public async Task<NoteDeFrais> UpdateAsync(int id, NoteDeFrais note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Note de frais avec Id={id} non trouvée.");


            var projet = await _projetRepository.GetByIdAsync(note.ProjetId.Value);
            if (projet == null)
                throw new KeyNotFoundException($"Projet avec Id={note.ProjetId} non trouvé.");

            // Mise à jour des propriétés autorisées
            existing.DateSoumission = note.DateSoumission;
            existing.Statut = note.Statut;
            existing.ProjetId = note.ProjetId;
            existing.Lignes = note.Lignes;

            return await _repository.UpdateAsync(existing);
        }

        public async Task<NoteDeFrais> UpdateStatus(int managerId, NoteDeFrais note)
        {
            var manager = await _userRepository.GetUserByIdAsync(managerId);
            if (manager == null)
                throw new ArgumentException("Manager introuvable dans la liste des utilisateurs.");

            var user = await _userRepository.GetUserByIdAsync(note.UserId);
            if (user.ManagerId != managerId)
                throw new ArgumentException("Vous n'avez pas le droit de changer l'état de cette note.");

            var existingNote = await _repository.GetByIdAsync(note.Id);
            if (existingNote == null)
                throw new KeyNotFoundException($"Note de frais avec Id={note.Id} introuvable.");

            if (existingNote.Statut != StatutNoteFrais.EnAttente)
                throw new InvalidOperationException("Le statut de cette note a déjà été traité.");

            if (note.Statut != StatutNoteFrais.Approuvee && note.Statut != StatutNoteFrais.Refusee)
                throw new ArgumentException("Le statut fourni doit être Approuvée ou Refusée.");

            existingNote.Statut = note.Statut;
            if (!string.IsNullOrEmpty(note.commentaireMananger))
            {
                existingNote.commentaireMananger = note.commentaireMananger;
            }

            return await _repository.UpdateAsync(existingNote);
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var success = await _repository.DeleteAsync(id);
            if (!success)
                throw new KeyNotFoundException($"Note de frais avec Id={id} non trouvée.");

            return true;
        }
    }
}
