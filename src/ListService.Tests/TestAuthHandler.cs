using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ListService.Tests;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var ownerId = ResolveOwnerIdFromHeader();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, ownerId),
            new Claim("sub", ownerId),
            new Claim("preferred_username", ownerId),
            new Claim("azp", "shopisel-list-api")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string ResolveOwnerIdFromHeader()
    {
        if (!Request.Headers.TryGetValue("X-Test-User", out StringValues values))
        {
            return "integration-test-user";
        }

        var candidate = values.ToString().Trim();
        return string.IsNullOrWhiteSpace(candidate) ? "integration-test-user" : candidate;
    }
}
