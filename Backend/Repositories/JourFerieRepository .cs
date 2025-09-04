using Microsoft.EntityFrameworkCore;
using MonBackend.Models;
using MonBackend.Data;
using MonBackend.Repositories.Interfaces;

namespace MonBackend.Repositories;

public class JourFerieRepository : IJourFerieRepository
{
    private readonly AppDbContext _context;

    public JourFerieRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<JourFerie>> GetAllAsync()
    {
        return await _context.JourFeries
            .OrderBy(j => j.Date)
            .ToListAsync();
    }

    public async Task<JourFerie?> GetByIdAsync(int id)
    {
        return await _context.JourFeries.FindAsync(id);
    }

    public async Task<List<JourFerie>> GetByYearAsync(int year)
    {
        return await _context.JourFeries
            .Where(j => j.Date.Year == year)
            .OrderBy(j => j.Date)
            .ToListAsync();
    }

    public async Task<List<JourFerie>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
    {
        return await _context.JourFeries
            .Where(j => j.Date >= dateDebut && j.Date <= dateFin)
            .OrderBy(j => j.Date)
            .ToListAsync();
    }

    public async Task<bool> IsJourFerieAsync(DateTime date)
    {
        return await _context.JourFeries
            .AnyAsync(j => j.Date.Date == date.Date);
    }

    public async Task<JourFerie> CreateAsync(JourFerie jourFerie)
    {
        _context.JourFeries.Add(jourFerie);
        await _context.SaveChangesAsync();
        return jourFerie;
    }

    public async Task<JourFerie?> UpdateAsync(JourFerie jourFerie)
    {
        _context.JourFeries.Update(jourFerie);
        await _context.SaveChangesAsync();
        return jourFerie;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var jourFerie = await _context.JourFeries.FindAsync(id);
        if (jourFerie == null) return false;

        _context.JourFeries.Remove(jourFerie);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(DateTime date)
    {
        return await _context.JourFeries
            .AnyAsync(j => j.Date.Date == date.Date);
    }

    public async Task<int> CountJoursFeriesAsync(DateTime dateDebut, DateTime dateFin)
    {
        return await _context.JourFeries
            .Where(j => j.Date >= dateDebut && j.Date <= dateFin)
            .CountAsync();
    }
}