using MonBackend.Models;
using MonBackend.DTOs;
using MonBackend.Repositories.Interfaces;

namespace MonBackend.Services;

public interface ITarifKmService
{
    Task<IEnumerable<TarifKm>> GetAllAsync();
    Task<TarifKm?> GetByIdAsync(int id);
    Task<TarifKm?> GetByCategorieAsync(string categorie);
    Task<TarifKm> CreateAsync(TarifKm tarif);
    Task<TarifKm?> UpdateAsync(int id,TarifKm tarif);
    Task<bool> DeleteAsync(int id);
}

