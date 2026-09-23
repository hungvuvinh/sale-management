using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SaleManagement.Api.Models;

namespace SaleManagement.Api.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public HealthController(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    [HttpGet("healthz")]
    public IActionResult GetHealth()
    {
        return Ok(new HealthStatusDto(
            Status: "healthy",
            Timestamp: DateTime.UtcNow,
            Environment: _env.EnvironmentName,
            DatabaseConnected: true
        ));
    }

    [HttpGet("readyz")]
    public async Task<IActionResult> GetReadiness()
    {
        var connStr = _config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=sale_management;Username=visssoft;Password=visssoft_dev_2026";

        bool dbConnected = false;
        try
        {
            using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            var result = await conn.ExecuteScalarAsync<int>("SELECT 1;");
            dbConnected = result == 1;
        }
        catch
        {
            dbConnected = false;
        }

        if (!dbConnected)
        {
            return StatusCode(503, new HealthStatusDto(
                Status: "unhealthy",
                Timestamp: DateTime.UtcNow,
                Environment: _env.EnvironmentName,
                DatabaseConnected: false
            ));
        }

        return Ok(new HealthStatusDto(
            Status: "ready",
            Timestamp: DateTime.UtcNow,
            Environment: _env.EnvironmentName,
            DatabaseConnected: true
        ));
    }
}
