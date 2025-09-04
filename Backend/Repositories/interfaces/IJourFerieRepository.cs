using Microsoft.EntityFrameworkCore;
using MonBackend.Models;
using MonBackend.Data;

namespace MonBackend.Repositories.Interfaces;

public interface IJourFerieRepository
{
    Task<List<JourFerie>> GetAllAsync();
    Task<JourFerie?> GetByIdAsync(int id);
    Task<List<JourFerie>> GetByYearAsync(int year);
    Task<List<JourFerie>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin);
    Task<bool> IsJourFerieAsync(DateTime date);
    Task<JourFerie> CreateAsync(JourFerie jourFerie);
    Task<JourFerie?> UpdateAsync(JourFerie jourFerie);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(DateTime date);
    Task<int> CountJoursFeriesAsync(DateTime dateDebut, DateTime dateFin);
}