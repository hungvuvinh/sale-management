using SaleManagement.Api.Entities;

namespace SaleManagement.Api.Repositories;

public interface IAuthRepository
{
    Task<UserEntity?> GetUserByEmailAsync(string email);
    Task<UserEntity?> GetUserByIdAsync(long id);
    Task<bool> CheckDatabaseConnectionAsync();
}
