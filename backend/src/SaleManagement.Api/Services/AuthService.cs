using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;

namespace SaleManagement.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);
    Task<UserProfileDto?> GetProfileByIdAsync(long userId);
}

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IAuthRepository authRepository, IJwtService jwtService)
    {
        _authRepository = authRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email);
        if (user == null) return null;

        bool isValidPassword;
        try
        {
            isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        catch (Exception)
        {
            isValidPassword = false;
        }

        if (!isValidPassword) return null;

        var mainRole = user.UserRoles.FirstOrDefault();
        string roleName = mainRole?.Role?.Code ?? "User";
        long? storeId = mainRole?.StoreId;

        var profile = new UserProfileDto(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Phone ?? "",
            roleName,
            storeId,
            user.IsActive
        );

        var (token, expiresIn) = _jwtService.GenerateToken(profile);

        return new LoginResponseDto(token, "Bearer", expiresIn, profile);
    }

    public async Task<UserProfileDto?> GetProfileByIdAsync(long userId)
    {
        var user = await _authRepository.GetUserByIdAsync(userId);
        if (user == null) return null;

        var mainRole = user.UserRoles.FirstOrDefault();
        string roleName = mainRole?.Role?.Code ?? "User";
        long? storeId = mainRole?.StoreId;

        return new UserProfileDto(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Phone ?? "",
            roleName,
            storeId,
            user.IsActive
        );
    }
}
