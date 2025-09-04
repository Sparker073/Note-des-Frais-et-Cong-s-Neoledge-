using System;
using MonBackend.Repositories.Interfaces;
using MonBackend.Data;
using MonBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace MonBackend.Repositories;

public class DemandeCongeRepository : IDemandeCongeRepository
{
    private readonly AppDbContext _context;

    public DemandeCongeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DemandeCongé>> GetAllAsync()
    {
        return await _context.DemandesCongés
            .Include(d => d.Employe)
            .OrderByDescending(d => d.DateDemande)
            .ToListAsync();
    }

    public async Task<DemandeCongé?> GetByIdAsync(int id)
    {
        return await _context.DemandesCongés
            .Include(d => d.Employe)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<DemandeCongé>> GetByUserIdAsync(int userId)
    {
        return await _context.DemandesCongés
            .Include(d => d.Employe)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.DateDemande)
            .ToListAsync();
    }

    public async Task<List<DemandeCongé>> GetByManagerIdAsync(int managerId)
    {
        return await _context.DemandesCongés
            .Include(d => d.Employe)
            .Where(d => d.Employe.ManagerId == managerId)
            .OrderByDescending(d => d.DateDemande)
            .ToListAsync();
    }

    public async Task<List<DemandeCongé>> GetByStatutAsync(StatutDemande statut)
    {
        return await _context.DemandesCongés    
            .Include(d => d.Employe)
            .Where(d => d.Statut == statut)
            .OrderByDescending(d => d.DateDemande)
            .ToListAsync();
    }

    public async Task<List<DemandeCongé>> GetByDateRangeAsync(DateTime dateDebut, DateTime dateFin)
    {
        return await _context.DemandesCongés
            .Include(d => d.Employe)
            .Where(d => d.DateDebut <= dateFin && d.DateFin >= dateDebut)
            .OrderByDescending(d => d.DateDemande)
            .ToListAsync();
    }
    //checking the date leave request that conflicts with other leave requests 
    public async Task<bool> HasConflictAsync(int userId, DateTime dateDebut, DateTime dateFin, int? excludeId = null)
    {
        var query = _context.DemandesCongés
            .Where(d => d.UserId == userId && 
                       d.DateDebut <= dateFin && 
                       d.DateFin >= dateDebut &&
                       d.Statut == StatutDemande.Approuve);

        if (excludeId.HasValue)
        {
            query = query.Where(d => d.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<DemandeCongé> CreateAsync(DemandeCongé demande)
    {
        _context.DemandesCongés.Add(demande);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(demande.Id);
    }

    public async Task<DemandeCongé?> UpdateAsync(DemandeCongé demande)
    {
        _context.DemandesCongés.Update(demande);
        await _context.SaveChangesAsync();
        return await GetByIdAsync(demande.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var demande = await _context.DemandesCongés.FindAsync(id);
        if (demande == null) return false;

        _context.DemandesCongés.Remove(demande);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetNombreJoursCongesByUserAndYearAsync(int userId, int year)
    {
        var demandes = await _context.DemandesCongés
            .Where(d => d.UserId == userId && 
                       d.DateDebut.Year == year && d.Statut == StatutDemande.Approuve)
            .ToListAsync();

        int totalJours = 0;
        foreach (var demande in demandes)
        {
            totalJours += CalculerNombreJoursOuvrables(demande.DateDebut, demande.DateFin);
        }

        return totalJours;
    }

    private int CalculerNombreJoursOuvrables(DateTime dateDebut, DateTime dateFin)
    {
        int jours = 0;
        for (DateTime date = dateDebut; date <= dateFin; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Sunday)
            {
                jours++;
            }
        }
        return jours;
    }
}