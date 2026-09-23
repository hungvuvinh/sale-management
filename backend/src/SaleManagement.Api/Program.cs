using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using SaleManagement.Api.Data;
using SaleManagement.Api.Hubs;
using SaleManagement.Api.Middleware;
using SaleManagement.Api.Options;
using SaleManagement.Api.Repositories;
using SaleManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var auditKey = builder.Configuration["AuditEncryption:Key"] ?? "sale_management_audit_key_2026_v1_32bytes";
builder.Services.AddSingleton(new AuditEncryptionService(auditKey));

// Add Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection(RateLimitOptions.SectionName));
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
var redisConfiguration = ConfigurationOptions.Parse(redisConnectionString);
redisConfiguration.AbortOnConnectFail = false;
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConfiguration));
builder.Services.AddSingleton<IRateLimitStore, RedisRateLimitStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sale Management API",
        Version = "v1",
        Description = "API Quản lý Bán hàng & Chuỗi Cửa hàng Đa Chi Nhánh (C# ASP.NET Core)"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token theo cú pháp: Bearer {your_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Entity Framework Core DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=sale_management;Username=visssoft;Password=visssoft_dev_2026";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Repositories
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockOrderRepository, StockOrderRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// Register Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<StoresService>();
builder.Services.AddScoped<ProductsService>();
builder.Services.AddScoped<PricingEngineService>();
builder.Services.AddScoped<StockOrdersService>();
builder.Services.AddScoped<OrdersService>();
builder.Services.AddScoped<DeliveryService>();
builder.Services.AddScoped<CommissionsService>();
builder.Services.AddScoped<MarketingService>();
builder.Services.AddScoped<ChatService>();

// Configure JWT Authentication
var secretKey = builder.Configuration["Jwt:Secret"] ?? "your_super_secret_jwt_key_change_in_production_2026";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "SaleManagement.Api";
var audience = builder.Configuration["Jwt:Audience"] ?? "SaleManagement.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sale Management API v1");
    });
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<RedisRateLimitingMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
