using MonBackend.Models;
using MonBackend.DTOs;

namespace MonBackend.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(CreateUserDto createDto);
    Task<User?> UpdateUserAsync(int id, UpdateUserDto updateDto);
    Task<bool> DeleteUserAsync(int id);
    Task<List<User>> GetSubordonnesAsync(int managerId);
}