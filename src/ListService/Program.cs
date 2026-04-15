using ListService.Data;
using ListService.Endpoints;
using ListService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("ListService");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'ConnectionStrings:ListService' is required.");
}

builder.Services.AddDbContext<ListServiceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var keycloakAuthority = builder.Configuration["Keycloak:Authority"];
var keycloakAudience = builder.Configuration["Keycloak:Audience"];
var keycloakAuthorizedParties =
    builder.Configuration.GetSection("Keycloak:AuthorizedParties").Get<string[]>();
var keycloakRequireHttpsMetadata =
    builder.Configuration.GetValue("Keycloak:RequireHttpsMetadata", true);

if (string.IsNullOrWhiteSpace(keycloakAuthority))
{
    throw new InvalidOperationException("Configuration 'Keycloak:Authority' is required.");
}

if (string.IsNullOrWhiteSpace(keycloakAudience) &&
    (keycloakAuthorizedParties is null || keycloakAuthorizedParties.Length == 0))
{
    throw new InvalidOperationException(
        "Configuration 'Keycloak:Audience' or 'Keycloak:AuthorizedParties' is required.");
}

var normalizedAuthority = keycloakAuthority.TrimEnd('/');
var allowedAuthorizedParties = (keycloakAuthorizedParties is { Length: > 0 }
        ? keycloakAuthorizedParties
        : new[] { keycloakAudience! })
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Select(value => value.Trim())
    .ToHashSet(StringComparer.Ordinal);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Preserve original JWT claim names (e.g., "sub") instead of mapping to WS-Fed types.
        options.MapInboundClaims = false;
        options.Authority = normalizedAuthority;
        options.RequireHttpsMetadata = keycloakRequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = normalizedAuthority,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "preferred_username"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var authorizedParty = context.Principal?.FindFirst("azp")?.Value;
                if (string.IsNullOrWhiteSpace(authorizedParty) ||
                    !allowedAuthorizedParties.Contains(authorizedParty))
                {
                    context.Fail(
                        $"Invalid 'azp' claim. Expected one of '{string.Join(", ", allowedAuthorizedParties)}', got '{authorizedParty ?? "<missing>"}'.");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Register custom services
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

await InitializeDatabaseAsync(app);

// Map Endpoint slices
app.MapListEndpoints();

await app.RunAsync();

static async Task InitializeDatabaseAsync(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ListServiceDbContext>();
    if (dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
    {
        await dbContext.Database.MigrateAsync();
        return;
    }

    await dbContext.Database.EnsureCreatedAsync();
}

public partial class Program;
