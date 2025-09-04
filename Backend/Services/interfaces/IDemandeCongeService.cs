using MonBackend.Models;
using MonBackend.DTOs;

namespace MonBackend.Services.Interfaces;

public interface IDemandeCongeService
{
    Task<List<DemandeCongeResponseDto>> GetAllDemandesAsync();
    Task<DemandeCongeResponseDto?> GetDemandeByIdAsync(int id);
    Task<List<DemandeCongeResponseDto>> GetDemandesByUserIdAsync(int userId);
    Task<List<DemandeCongeResponseDto>> GetDemandesByManagerIdAsync(int managerId);
    Task<List<DemandeCongeResponseDto>> GetDemandesByStatutAsync(StatutDemande statut);
    Task<(DemandeCongeResponseDto demande, List<string> joursFeries)> CreateDemandeAsync(int userId, CreateDemandeCongeDto createDto);
    Task<(DemandeCongeResponseDto demande, List<string> joursFeries)> UpdateDemandeAsync(int id, UpdateDemandeCongeDto updateDto);
    Task<DemandeCongeResponseDto?> UpdateStatutDemandeAsync(int id, int managerId, UpdateStatutDemandeDto updateDto);
    Task<bool> DeleteDemandeAsync(int id);
    Task<int> GetSoldeCongesAsync(int userId, int year);
}