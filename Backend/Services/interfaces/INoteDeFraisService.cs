using System.Collections.Generic;
using System.Threading.Tasks;
using MonBackend.Models;

namespace MonBackend.Services.Interfaces;

public interface INoteDeFraisService
{
    Task<IEnumerable<NoteDeFrais>> GetAllAsync();
    Task<NoteDeFrais?> GetByIdAsync(int id);
    Task<IEnumerable<NoteDeFrais>> GetByUserIdAsync(int userId);
    Task<IEnumerable<NoteDeFrais>> GetByManagerIdAsync(int managerId);
    Task<IEnumerable<NoteDeFrais>> GetByProjetIdAsync(int projetId);
    Task<NoteDeFrais> CreateAsync(NoteDeFrais note);
    Task<NoteDeFrais> UpdateAsync(int id, NoteDeFrais note);
    Task<NoteDeFrais> UpdateStatus(int managerId, NoteDeFrais note);
    Task<bool> DeleteAsync(int id);
}

