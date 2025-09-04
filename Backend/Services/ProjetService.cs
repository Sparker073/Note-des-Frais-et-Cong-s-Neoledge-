using MonBackend.Models;
using MonBackend.Repositories.Interfaces;
using MonBackend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonBackend.Services;

public class ProjetService : IProjetService
{
    private readonly IProjetRepository _repository;

    public ProjetService(IProjetRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Projet>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Projet> GetByIdAsync(int id)
    {
        var projet = await _repository.GetByIdAsync(id);
        if (projet == null)
            throw new KeyNotFoundException($"Projet avec l'ID {id} non trouvé.");
        return projet;
    }

    public async Task<Projet> CreateAsync(Projet projet)
    {
        var existing = await _repository.GetByNameAsync(projet.Nom);
        if (existing != null)
            throw new ArgumentException($"Un projet avec le nom '{projet.Nom}' existe déjà.");
        
        if (string.IsNullOrWhiteSpace(projet.Nom))
            throw new ArgumentException("Le nom du projet est obligatoire.");
        return await _repository.CreateAsync(projet);
    }

    public async Task<Projet> UpdateAsync(int id, Projet projet)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Projet avec l'ID {id} non trouvé.");

        existing.Nom = projet.Nom;
        existing.Description = projet.Description;

        return await _repository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
            throw new KeyNotFoundException($"Projet avec l'ID {id} non trouvé.");
    }
}

