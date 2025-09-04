using Microsoft.EntityFrameworkCore;
using MonBackend.Models;
using MonBackend.Data;
using MonBackend.Repositories.Interfaces;

namespace MonBackend.Repositories;

public class ProjetRepository : IProjetRepository
{
    private readonly AppDbContext _context;

    public ProjetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Projet>> GetAllAsync()
    {
        return await _context.Projets.ToListAsync();
    }

    public async Task<Projet?> GetByIdAsync(int id)
    {
        return await _context.Projets.FindAsync(id);
    }

    public async Task<Projet?> GetByNameAsync(string nom)
    {
        return await _context.Projets.FirstOrDefaultAsync(p => p.Nom.ToLower() == nom.ToLower());
    }

    public async Task<Projet> CreateAsync(Projet projet)
    {
        _context.Projets.Add(projet);
        await _context.SaveChangesAsync();
        return projet;
    }

    public async Task<Projet?> UpdateAsync(Projet projet)
    {
        var existing = await _context.Projets.FindAsync(projet.Id);
        if (existing == null) return null;

        existing.Nom = projet.Nom;
        existing.Description = projet.Description;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var projet = await _context.Projets.FindAsync(id);
        if (projet == null) return false;

        _context.Projets.Remove(projet);
        await _context.SaveChangesAsync();
        return true;
    }
}
