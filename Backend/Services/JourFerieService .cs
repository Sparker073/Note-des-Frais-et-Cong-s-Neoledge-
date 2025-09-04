using MonBackend.Models;
using MonBackend.DTOs;
using MonBackend.Repositories.Interfaces;

namespace MonBackend.Services.Interfaces;

public class JourFerieService : IJourFerieService
{
    private readonly IJourFerieRepository _jourFerieRepository;

    public JourFerieService(IJourFerieRepository jourFerieRepository)
    {
        _jourFerieRepository = jourFerieRepository;
    }

    public async Task<List<JourFerie>> GetAllJoursFeriesAsync()
    {
        return await _jourFerieRepository.GetAllAsync();
    }

    public async Task<JourFerie?> GetJourFerieByIdAsync(int id)
    {
        return await _jourFerieRepository.GetByIdAsync(id);
    }

    public async Task<List<JourFerie>> GetJoursFeriesByYearAsync(int year)
    {
        return await _jourFerieRepository.GetByYearAsync(year);
    }

    public async Task<JourFerie> CreateJourFerieAsync(CreateJourFerieDto createDto)
    {
        // Vérifier si le jour férié existe déjà
        if (await _jourFerieRepository.ExistsAsync(createDto.Date))
            throw new InvalidOperationException("Un jour férié existe déjà pour cette date");

        var jourFerie = new JourFerie
        {
            Date = createDto.Date,
            Description = createDto.Description
        };

        return await _jourFerieRepository.CreateAsync(jourFerie);
    }

    public async Task<JourFerie?> UpdateJourFerieAsync(int id, UpdateJourFerieDto updateDto)
    {
        var jourFerie = await _jourFerieRepository.GetByIdAsync(id);
        if (jourFerie == null) return null;

        // Si la date est modifiée, vérifier qu'elle n'existe pas déjà
        if (updateDto.Date.HasValue && updateDto.Date.Value != jourFerie.Date)
        {
            if (await _jourFerieRepository.ExistsAsync(updateDto.Date.Value))
                throw new InvalidOperationException("Un jour férié existe déjà pour cette date");
            
            jourFerie.Date = updateDto.Date.Value;
        }

        if (updateDto.Description != null)
            jourFerie.Description = updateDto.Description;

        return await _jourFerieRepository.UpdateAsync(jourFerie);
    }

    public async Task<bool> DeleteJourFerieAsync(int id)
    {
        return await _jourFerieRepository.DeleteAsync(id);
    }

    public async Task<bool> IsJourFerieAsync(DateTime date)
    {
        return await _jourFerieRepository.IsJourFerieAsync(date);
    }
}