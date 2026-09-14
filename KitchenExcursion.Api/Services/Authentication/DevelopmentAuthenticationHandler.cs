using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace KitchenExcursion.Api.Services.Authentication;

public sealed class DevelopmentAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Development";

    private const string EntraObjectIdClaim =
        "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Development only: provide a predictable local identity so Kitchen
        // can exercise authenticated UI/API paths without requiring Entra.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "development-user"),
            new Claim(ClaimTypes.Name, "Randy"),
            new Claim(ClaimTypes.Email, "Development User"),
            new Claim(ClaimTypes.GivenName, "Randy"),
            new Claim(EntraObjectIdClaim, "development-user"),
            new Claim("oid", "development-user"),
            new Claim("preferred_username", "Development User"),
            new Claim("given_name", "Randy")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
