using Microsoft.EntityFrameworkCore;
using SaleManagement.Api.Data;
using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;

    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Store)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
    }

    public async Task<UserEntity?> GetUserByIdAsync(long id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Store)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<bool> CheckDatabaseConnectionAsync()
    {
        return await _context.Database.CanConnectAsync();
    }
}
