using MonBackend.Models;
using MonBackend.DTOs;
using MonBackend.Repositories.Interfaces;

namespace MonBackend.Services.Interfaces;

public interface IJourFerieService
{
    Task<List<JourFerie>> GetAllJoursFeriesAsync();
    Task<JourFerie?> GetJourFerieByIdAsync(int id);
    Task<List<JourFerie>> GetJoursFeriesByYearAsync(int year);
    Task<JourFerie> CreateJourFerieAsync(CreateJourFerieDto createDto);
    Task<JourFerie?> UpdateJourFerieAsync(int id, UpdateJourFerieDto updateDto);
    Task<bool> DeleteJourFerieAsync(int id);
    Task<bool> IsJourFerieAsync(DateTime date);
}
