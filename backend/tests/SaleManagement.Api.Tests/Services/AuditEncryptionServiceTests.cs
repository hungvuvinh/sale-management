using SaleManagement.Api.Services;
using Xunit;

namespace SaleManagement.Api.Tests.Services;

public sealed class AuditEncryptionServiceTests
{
    [Fact]
    public void Encrypt_And_Decrypt_RoundTripsPayload()
    {
        var service = new AuditEncryptionService("0123456789ABCDEF0123456789ABCDEF");
        const string payload = "{\"orderId\":123,\"status\":\"COMPLETED\"}";

        var encrypted = service.Encrypt(payload);

        Assert.NotEqual(payload, encrypted);
        Assert.Equal(payload, service.Decrypt(encrypted));
    }

    [Fact]
    public void Encrypt_UsesUniqueCipherText_ForSamePayload()
    {
        var service = new AuditEncryptionService("FEDCBA9876543210FEDCBA9876543210");
        const string payload = "{\"value\":42}";

        var first = service.Encrypt(payload);
        var second = service.Encrypt(payload);

        Assert.NotEqual(first, second);
        Assert.Equal(payload, service.Decrypt(first));
        Assert.Equal(payload, service.Decrypt(second));
    }
}
