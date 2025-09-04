using System.Collections.Generic;
using MonBackend.Models;

namespace MonBackend.Repositories.Interfaces
{
    public interface ILigneNoteFraisRepository
    {
        Task<IEnumerable<LigneNoteFrais>> GetAllAsync();
        Task<LigneNoteFrais?> GetByIdAsync(int id);
        Task<IEnumerable<LigneNoteFrais>> GetByNoteDeFraisIdAsync(int noteId);
        Task<LigneNoteFrais> CreateAsync(LigneNoteFrais ligne);
        Task<LigneNoteFrais?> UpdateAsync(LigneNoteFrais ligne);
        Task<LigneNoteFrais?> DeleteAsync(int id);
    }
}
