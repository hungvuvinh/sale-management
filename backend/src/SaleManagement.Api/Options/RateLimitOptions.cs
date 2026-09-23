namespace SaleManagement.Api.Options;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int PublicRequestsPerMinute { get; init; } = 60;
    public int StaffRequestsPerMinute { get; init; } = 300;
    public int AdminRequestsPerMinute { get; init; } = 1000;
}