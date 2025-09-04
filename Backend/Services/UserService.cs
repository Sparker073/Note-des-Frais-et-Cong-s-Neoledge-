using MonBackend.Models;
using MonBackend.DTOs;
using MonBackend.Repositories.Interfaces;
using MonBackend.Services.Interfaces;

namespace MonBackend.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }
    
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetUserByIdAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetUserByEmailAsync(email);
    }

    public async Task<User> CreateUserAsync(CreateUserDto createDto)
    {
        // Vérifier si l'email existe déjà
        var existingUser = await _userRepository.GetUserByEmailAsync(createDto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Un utilisateur avec cet email existe déjà.");

        // Valider l'existence du manager
        if (createDto.ManagerId.HasValue)
        {
            var manager = await _userRepository.GetUserByIdAsync(createDto.ManagerId.Value);
            if (manager == null)
                throw new InvalidOperationException("Le manager spécifié n'existe pas.");
        }

        // Validation manuelle du rôle 
        var validRoles = new[] { "admin", "employe" };
        if (string.IsNullOrWhiteSpace(createDto.Role) || !validRoles.Contains(createDto.Role.ToLower()))
        {
            throw new InvalidOperationException("Le rôle spécifié est invalide. Les rôles valides sont : Admin, Employe.");
        }

        var user = new User
        {
            Nom = createDto.Nom,
            Email = createDto.Email.ToLower(),
            MotDePasse = createDto.MotDePasse, 
            Role = createDto.Role.ToLower(),
            Poste = createDto.Poste,
            ManagerId = createDto.ManagerId
        };

        return await _userRepository.CreateUserAsync(user);
    }

    public async Task<User?> UpdateUserAsync(int id, UpdateUserDto updateDto)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return null;

        // Check if email already exists (excluding current user)   
        var existingUserWithEmail = await _userRepository.GetUserByEmailAsync(updateDto.Email);
        if (existingUserWithEmail != null && existingUserWithEmail.Id != id)
            throw new InvalidOperationException("Un autre utilisateur avec cet email existe déjà.");

        // Validate manager
        if (updateDto.ManagerId.HasValue)
        {
            // User cannot be their own manager
            if (updateDto.ManagerId.Value == id)
                throw new InvalidOperationException("Un utilisateur ne peut pas être son propre manager.");
            
            // Manager must exist
            var manager = await _userRepository.GetUserByIdAsync(updateDto.ManagerId.Value);
            if (manager == null)
                throw new InvalidOperationException("Le manager spécifié n'existe pas.");
            if (manager.ManagerId == id)
                throw new InvalidOperationException("Relation de management circulaire détectée.");
        }

        user.Nom = updateDto.Nom;
        user.Email = updateDto.Email.ToLower();
        user.Role = updateDto.Role;
        user.Poste = updateDto.Poste;
        user.ManagerId = updateDto.ManagerId;
        

        return await _userRepository.UpdateUserAsync(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteUserAsync(id);
    }

    public async Task<List<User>> GetSubordonnesAsync(int managerId)
    {
        // Vérifie d'abord que le manager existe (optionnel mais recommandé)
        var manager = await _userRepository.GetUserByIdAsync(managerId);
        if (manager == null)
            throw new InvalidOperationException("Le manager spécifié n'existe pas.");

        // Récupère les utilisateurs dont le managerId correspond à l'ID donné
        return await _userRepository.GetSubordonnesByManagerIdAsync(managerId);
    }

}

