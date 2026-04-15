using Sertifika.Context;
using Sertifika.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sertifika.EntityServices;

public class UserEntityService : IUserEntityService
{
    private readonly AppDbContext _context;

    public UserEntityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

    public async Task<IEnumerable<User>> GetAllAsync()
        => await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .ToListAsync();

    public void Add(User user) => _context.Users.Add(user);

    public void Update(User user) => _context.Users.Update(user);

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public async Task<int> CountActiveAdminsAsync(int? excludeId = null)
        => await _context.Users.CountAsync(u =>
            u.IsActive && u.Role == UserRole.Admin &&
            (excludeId == null || u.Id != excludeId));
}
