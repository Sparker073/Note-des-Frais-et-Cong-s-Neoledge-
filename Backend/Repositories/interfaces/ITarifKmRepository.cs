using System.Collections.Generic;
using MonBackend.Models;

namespace MonBackend.Repositories.Interfaces;


public interface ITarifKmRepository
{
    Task<IEnumerable<TarifKm>> GetAllAsync();
    Task<TarifKm?> GetByIdAsync(int id);
    Task<TarifKm?> GetByCategorieAsync(string categorie);
    Task<TarifKm> CreateAsync(TarifKm tarif);
    Task<TarifKm?> UpdateAsync(TarifKm tarif);
    Task<bool> DeleteAsync(int id);
}