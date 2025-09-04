using Microsoft.EntityFrameworkCore;
using MonBackend.Models;
using MonBackend.Data;

namespace MonBackend.Repositories.Interfaces;

public interface IDemandeCongeRepository 
{
    Task<List<DemandeCongé>> GetAllAsync();
    Task<DemandeCongé?> GetByIdAsync(int id);
    Task<List<DemandeCongé>> GetByUserIdAsync(int userId);
    Task<List<DemandeCongé>> GetByManagerIdAsync(int managerId);
    Task<List<DemandeCongé>> GetByStatutAsync(StatutDemande statut);
    Task<List<DemandeCongé>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
    Task<bool> HasConflictAsync(int userId, DateTime dateDebut, DateTime dateFin, int? excludeId = null);
    Task<DemandeCongé> CreateAsync(DemandeCongé demande);
    Task<DemandeCongé?> UpdateAsync(DemandeCongé demande);
    Task<bool> DeleteAsync(int id);
    Task<int> GetNombreJoursCongesByUserAndYearAsync(int userId, int year);
}