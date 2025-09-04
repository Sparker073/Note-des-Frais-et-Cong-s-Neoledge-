    using MonBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonBackend.Repositories.Interfaces;

public interface IProjetRepository
{
    Task<IEnumerable<Projet>> GetAllAsync();
    Task<Projet?> GetByIdAsync(int id);
    Task<Projet?> GetByNameAsync(string nom);
    Task<Projet> CreateAsync(Projet projet);
    Task<Projet?> UpdateAsync(Projet projet);
    Task<bool> DeleteAsync(int id);
}

