using Microsoft.EntityFrameworkCore;
using MonBackend.Models;
using MonBackend.Repositories.Interfaces;
using MonBackend.Data;

namespace MonBackend.Repositories;

public class TarifKmRepository : ITarifKmRepository
{
    private readonly AppDbContext _context;

    public TarifKmRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TarifKm>> GetAllAsync()
    {
        return await _context.TarifsKm.ToListAsync();
    }

    public async Task<TarifKm?> GetByIdAsync(int id)
    {
        return await _context.TarifsKm.FindAsync(id);
    }

    public async Task<TarifKm?> GetByCategorieAsync(string categorie)
    {
        return await _context.TarifsKm
        .FirstOrDefaultAsync(t => t.CategorieVehicule.ToLower() == categorie.ToLower());
    }

    public async Task<TarifKm> CreateAsync(TarifKm tarif)
    {
        _context.TarifsKm.Add(tarif);
        await _context.SaveChangesAsync();
        return tarif;
    }

    public async Task<TarifKm?> UpdateAsync(TarifKm tarif)
    {
        var existing = await _context.TarifsKm.FindAsync(tarif.Id);
        if (existing == null)
            return null;
        existing.CategorieVehicule = tarif.CategorieVehicule;
        existing.TarifParKm = tarif.TarifParKm;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.TarifsKm.FindAsync(id);
        if (entity == null)
            return false;

        _context.TarifsKm.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
