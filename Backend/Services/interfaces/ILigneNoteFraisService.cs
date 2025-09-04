using System.Collections.Generic;
using System.Threading.Tasks;
using MonBackend.Models;

namespace MonBackend.Services.Interfaces
{
    public interface ILigneNoteFraisService
    {
        Task<IEnumerable<LigneNoteFrais>> GetAllAsync();
        Task<LigneNoteFrais?> GetByIdAsync(int id);
        Task<IEnumerable<LigneNoteFrais>> GetByNoteDeFraisIdAsync(int noteId);
        Task<LigneNoteFrais> CreateAsync(LigneNoteFrais ligne, int userId);
        Task<LigneNoteFrais?> UpdateAsync(LigneNoteFrais ligne, int userId);
        Task<LigneNoteFrais?> DeleteAsync(int id, int userId);
    }
}
