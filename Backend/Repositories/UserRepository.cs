using MonBackend.Models;
using MonBackend.Data;
using Microsoft.EntityFrameworkCore;
using MonBackend.Repositories.Interfaces;

namespace MonBackend.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Manager)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<User>> GetSubordonnesByManagerIdAsync(int managerId)
    {
        return await _context.Users
            .Where(u => u.ManagerId == managerId)
            .ToListAsync();
    }
}