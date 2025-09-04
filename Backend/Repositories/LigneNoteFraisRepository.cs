using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonBackend.Models;
using MonBackend.Data;
using MonBackend.Repositories.Interfaces;

namespace MonBackend.Repositories
{
    public class LigneNoteFraisRepository : ILigneNoteFraisRepository
    {
        private readonly AppDbContext _context;

        public LigneNoteFraisRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LigneNoteFrais>> GetAllAsync()
        {
            return await _context.LignesNotesFrais
                .Include(l => l.TarifKm)
                .ToListAsync();
        }

        public async Task<LigneNoteFrais?> GetByIdAsync(int id)
        {
            return await _context.LignesNotesFrais
                .Include(l => l.NoteDeFrais) // ✅ IMPORTANT : Incluez la relation
                .FirstOrDefaultAsync(l => l.Id == id);
        }
        public async Task<IEnumerable<LigneNoteFrais>> GetByNoteDeFraisIdAsync(int noteId)
        {
            return await _context.LignesNotesFrais
                .Include(l => l.TarifKm)
                .Where(l => l.NoteDeFraisId == noteId)
                .ToListAsync();
        }

        public async Task<LigneNoteFrais> CreateAsync(LigneNoteFrais ligne)
        {
            _context.LignesNotesFrais.Add(ligne);
            await _context.SaveChangesAsync();
            return ligne;
        }

        public async Task<LigneNoteFrais?> UpdateAsync(LigneNoteFrais ligne)
        {
            var existing = await _context.LignesNotesFrais.FindAsync(ligne.Id);
            if (existing == null)
                return null;

            existing.Date = ligne.Date;
            existing.Description = ligne.Description;
            existing.Montant = ligne.Montant;
            existing.JustificatifPath = ligne.JustificatifPath;
            existing.TarifKmId = ligne.TarifKmId;
            existing.DistanceKm = ligne.DistanceKm;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<LigneNoteFrais>? DeleteAsync(int id)
        {
            var ligne = await _context.LignesNotesFrais.FindAsync(id);
            if (ligne == null)
                return null;

            _context.LignesNotesFrais.Remove(ligne);
            await _context.SaveChangesAsync();
            return ligne;
        }
    }
}
