using MonBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonBackend.Services.Interfaces;

public interface IProjetService
{
    Task<IEnumerable<Projet>> GetAllAsync();
    Task<Projet> GetByIdAsync(int id);
    Task<Projet> CreateAsync(Projet projet);
    Task<Projet> UpdateAsync(int id, Projet projet);
    Task DeleteAsync(int id);
}

