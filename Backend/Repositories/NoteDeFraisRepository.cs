using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MonBackend.Models;
using MonBackend.Data;
using MonBackend.Repositories.Interfaces;

namespace MonBackend.Repositories
{
    public class NoteDeFraisRepository : INoteDeFraisRepository
    {
        private readonly AppDbContext _context;

        public NoteDeFraisRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NoteDeFrais>> GetAllAsync()
        {
            return await _context.NotesDeFrais
                .Include(n => n.Lignes)
                .Include(n => n.Projet)
                .ToListAsync();
        }

        public async Task<NoteDeFrais?> GetByIdAsync(int id)
        {
            return await _context.NotesDeFrais
                .Include(n => n.Lignes)
                .Include(n => n.Projet)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<NoteDeFrais>> GetByUserIdAsync(int userId)
        {
            return await _context.NotesDeFrais
                .Include(n => n.Lignes)
                .Include(n => n.Projet)
                .Where(n => n.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<NoteDeFrais>> GetByProjetIdAsync(int projetId)
        {
            return await _context.NotesDeFrais
                .Include(n => n.Lignes)
                .Include(n => n.Projet)
                .Where(n => n.ProjetId == projetId)
                .ToListAsync();
        }

        public async Task<IEnumerable<NoteDeFrais>> GetByManagerIdAsync(int managerId)
        {
            return await _context.NotesDeFrais
                .Include(n => n.Lignes)
                .Include(n => n.Projet)
                .Where(n => n.Employe.ManagerId == managerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<NoteDeFrais>> GetByUserIdAndProjetIdAsync(int userId, int projetId)
        {
            return await _context.NotesDeFrais
                .Include(n => n.Lignes)
                .Include(n => n.Projet)
                .Where(n => n.UserId == userId && n.ProjetId == projetId)
                .ToListAsync();
        }


        public async Task<NoteDeFrais> CreateAsync(NoteDeFrais note)
        {
            _context.NotesDeFrais.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<NoteDeFrais?> UpdateAsync(NoteDeFrais note)
        {
            var existing = await _context.NotesDeFrais
                .Include(n => n.Lignes)
                .FirstOrDefaultAsync(n => n.Id == note.Id);

            if (existing == null)
                return null;

            existing.Statut = note.Statut;
            existing.DateSoumission = note.DateSoumission;
            existing.UserId = note.UserId;
            existing.ProjetId = note.ProjetId;
            existing.Lignes = note.Lignes;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var note = await _context.NotesDeFrais.FindAsync(id);
            if (note == null)
                return false;

            _context.NotesDeFrais.Remove(note);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
