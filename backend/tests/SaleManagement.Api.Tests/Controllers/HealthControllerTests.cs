using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using SaleManagement.Api.Controllers;
using SaleManagement.Api.Models;
using Xunit;

namespace SaleManagement.Api.Tests.Controllers;

public sealed class HealthControllerTests
{
    [Fact]
    public void GetHealth_ReturnsHealthyStatus()
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.EnvironmentName).Returns("Testing");
        var controller = new HealthController(new ConfigurationBuilder().Build(), environment.Object);

        var result = controller.GetHealth();

        var response = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        var health = Assert.IsType<HealthStatusDto>(response.Value);
        Assert.Equal("healthy", health.Status);
        Assert.Equal("Testing", health.Environment);
        Assert.True(health.DatabaseConnected);
    }

    [Fact]
    public async Task GetReadiness_ReturnsServiceUnavailable_WhenDatabaseCannotBeReached()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=127.0.0.1;Port=1;Database=sale_management;Username=test;Password=test;Timeout=1;Command Timeout=1"
            })
            .Build();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.EnvironmentName).Returns("Testing");
        var controller = new HealthController(configuration, environment.Object);

        var result = await controller.GetReadiness();

        var response = Assert.IsType<Microsoft.AspNetCore.Mvc.ObjectResult>(result);
        Assert.Equal(503, response.StatusCode);
        var health = Assert.IsType<HealthStatusDto>(response.Value);
        Assert.Equal("unhealthy", health.Status);
        Assert.False(health.DatabaseConnected);
    }
}