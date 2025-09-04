using System.Collections.Generic;
using System.Threading.Tasks;
using MonBackend.Models;

namespace MonBackend.Repositories.Interfaces;

public interface INoteDeFraisRepository
{
    Task<IEnumerable<NoteDeFrais>> GetAllAsync();
    Task<NoteDeFrais?> GetByIdAsync(int id);
    Task<IEnumerable<NoteDeFrais>> GetByUserIdAsync(int userId);
    Task<IEnumerable<NoteDeFrais>> GetByManagerIdAsync(int managerId);
    Task<IEnumerable<NoteDeFrais>> GetByProjetIdAsync(int projetId);
    Task<IEnumerable<NoteDeFrais>> GetByUserIdAndProjetIdAsync(int userId, int projetId);
    Task<NoteDeFrais> CreateAsync(NoteDeFrais note);
    Task<NoteDeFrais?> UpdateAsync(NoteDeFrais note);
    Task<bool> DeleteAsync(int id);
}

