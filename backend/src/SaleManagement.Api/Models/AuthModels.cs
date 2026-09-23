namespace SaleManagement.Api.Models;

public record LoginRequestDto(string Email, string Password);

public record LoginResponseDto(
    string Token,
    string TokenType,
    int ExpiresInSeconds,
    UserProfileDto User
);

public record UserProfileDto(
    long Id,
    string Email,
    string FullName,
    string Phone,
    string Role,
    long? StoreId,
    bool IsActive
);

public record HealthStatusDto(
    string Status,
    DateTime Timestamp,
    string Environment,
    bool DatabaseConnected
);
