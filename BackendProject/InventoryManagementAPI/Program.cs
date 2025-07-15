using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Repositories;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Services;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Middleware;
using InventoryManagementAPI.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InventoryManagementAPI.Hubs;
using AspNetCoreRateLimit;
using InventoryManagementAPI.Utilities;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.IdentityModel.Tokens.Jwt;
using Serilog;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;


var builder = WebApplication.CreateBuilder(args);

// builder.Host.UseSerilog((context, services, configuration) => configuration
//     .ReadFrom.Configuration(context.Configuration)
//     .ReadFrom.Services(services)
//     .Enrich.FromLogContext()
// );


builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// --- Add API Versioning ---
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true; // Reports API versions in response headers
    options.AssumeDefaultVersionWhenUnspecified = true; // Use default version if not specified
    options.DefaultApiVersion = new ApiVersion(1, 0); // Sets default to v1.0
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // Reads version from URL segment (e.g., /api/v1/)
});

// --- Add API Explorer for Swagger/Swagger UI ---
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Formats the API version as vMajor (e.g., v1)
    options.SubstituteApiVersionInUrl = true; // Replaces the version placeholder in the URL
});

// Register ApplicationDbContext with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.WithOrigins("http://127.0.0.1:5500", "http://localhost:5085", "http://127.0.0.1:4200", "http://localhost:4200", "http://127.0.0.1:8080", "http://localhost:8080")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

// Register OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Swagger to recognize versions
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddSignalR();

// Configure IpRateLimiting
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();

// Configure ClientRateLimiting
builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("ClientRateLimiting"));
builder.Services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();

// Inject the counter store and client id resolver
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

// Use a custom client ID resolver to get the user ID from the JWT token
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Register repositories
builder.Services.AddScoped<IRepository<int, Role>, RoleRepository>();
builder.Services.AddScoped<IRepository<int, AuditLog>, AuditLogRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryManagerRepository, InventoryManagerRepository>();
builder.Services.AddScoped<IInventoryProductRepository, InventoryProductRepository>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Register services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInventoryManagerService, InventoryManagerService>();
builder.Services.AddScoped<IInventoryProductService, InventoryProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// builder.Services.AddScoped<IFileStorageService>(provider =>
// {
//     var uploadBasePath = builder.Configuration["UploadSettings:BasePath"];
//     var auditLogService = provider.GetRequiredService<IAuditLogService>();
//     return new LocalFileStorageService(uploadBasePath, auditLogService);
// });

var client = new SecretClient(new Uri(builder.Configuration["AzureKeyVault:VaultUri"]), new DefaultAzureCredential());
KeyVaultSecret secret = await client.GetSecretAsync(builder.Configuration["AzureKeyVault:secretName"]);

builder.Services.AddScoped<IFileStorageService>(sp =>
    new BlobFileStorageService(
        secret.Value,
        builder.Configuration["AzureBlobStorage:ContainerName"],
        sp.GetRequiredService<IAuditLogService>()
    )
);


// --- Configure JWT Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
    };
    options.Events = new JwtBearerEvents
    {

        OnTokenValidated = async context =>
        {
            var tokenBlacklistService = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
            var jti = context.Principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (jti != null && await tokenBlacklistService.IsTokenBlacklisted(jti))
            {
                context.Fail("This token has been revoked.");
                return;
            }


            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var userId = context.Principal.GetUserId();
                if (userId.HasValue)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
                    claimsIdentity.AddClaim(new Claim("clientid", userId.Value.ToString()));
                }
            }
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});




var app = builder.Build();

//app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Build a swagger endpoint for each discovered API version
        foreach (var description in app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseIpRateLimiting();
app.UseClientRateLimiting();


app.UseAuthentication();

app.UseAuthorization();

app.MapHub<LowStockHub>("/lowstock-notifications");

app.UseMiddleware<RoleValidationMiddleware>();

app.MapControllers();

app.Run();