using MonBackend.Models;

namespace MonBackend.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int id);
    Task<List<User>> GetSubordonnesByManagerIdAsync(int managerId);
}
