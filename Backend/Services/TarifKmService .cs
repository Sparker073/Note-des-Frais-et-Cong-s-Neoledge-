using MonBackend.Models;
using MonBackend.Repositories.Interfaces;
using MonBackend.Services.Interfaces;

namespace MonBackend.Services;

public class TarifKmService : ITarifKmService
{
    private readonly ITarifKmRepository _repository;

    public TarifKmService(ITarifKmRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TarifKm>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<TarifKm> GetByIdAsync(int id)
    {
        var tarif = await _repository.GetByIdAsync(id);
        if (tarif == null)
            throw new KeyNotFoundException($"TarifKm avec Id={id} non trouvé.");
        return tarif;
    }

    public async Task<TarifKm> CreateAsync(TarifKm tarif)
    {
        var existtarif = await _repository.GetByCategorieAsync(tarif.CategorieVehicule);
        if (existtarif != null)
            throw new ArgumentException("Tarif de cette vehicule existe deja !.");
        if (string.IsNullOrWhiteSpace(tarif.CategorieVehicule))
            throw new ArgumentException("La catégorie du véhicule est obligatoire.");
        if (tarif.TarifParKm <= 0)
            throw new ArgumentException("Le tarif par km doit être supérieur à 0.");

        return await _repository.CreateAsync(tarif);
    }

    public async Task<TarifKm?> GetByCategorieAsync(string categorie)
    {
        return await _repository.GetByCategorieAsync(categorie);
    }

    public async Task<TarifKm?> UpdateAsync(int id, TarifKm tarif)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"TarifKm avec Id={id} non trouvé.");
        var existtarif = await _repository.GetByCategorieAsync(tarif.CategorieVehicule);
        if (existtarif != null)
            throw new ArgumentException("Tarif de cette vehicule existe deja !.");
        if (string.IsNullOrWhiteSpace(tarif.CategorieVehicule))
            throw new ArgumentException("La catégorie du véhicule est obligatoire.");
        if (tarif.TarifParKm <= 0)
            throw new ArgumentException("Le tarif par km doit être supérieur à 0.");

        existing.CategorieVehicule = tarif.CategorieVehicule;
        existing.TarifParKm = tarif.TarifParKm;

        return await _repository.UpdateAsync(existing) ?? throw new Exception("Erreur lors de la mise à jour.");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
            throw new KeyNotFoundException($"TarifKm avec Id={id} non trouvé pour suppression.");
        return true;
    }
}
