using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using KitchenExcursion.Api.Data;

namespace KitchenExcursion.Api.Services.Authentication;

public sealed class DevelopmentAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Development";

    private const string EntraObjectIdClaim =
        "http://schemas.microsoft.com/identity/claims/objectidentifier";

    private readonly LaUltimaExcursionDbContext _platformDb;

    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        LaUltimaExcursionDbContext platformDb)
        : base(options, logger, encoder)
    {
        _platformDb = platformDb;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Development only: impersonate the first configured platform user,
        // preferring the user that already has a default vehicle.
        //
        // This keeps local API tools (curl/Postman) inside the same household
        // authorization path as the production app without requiring CIAM.
        var user = await _platformDb.Users
            .AsNoTracking()
            .Where(u =>
                u.EntraObjectId != null &&
                u.EntraObjectId != "" &&
                u.DefaultHouseholdId != null)
            .OrderByDescending(u => u.DefaultVehicleId != null)
            .ThenBy(u => u.Id)
            .FirstOrDefaultAsync(Context.RequestAborted);

        if (user == null)
        {
            return AuthenticateResult.Fail(
                "No development platform user is available.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.EntraObjectId),
            new Claim(ClaimTypes.Name, "Development User"),
            new Claim(EntraObjectIdClaim, user.EntraObjectId),
            new Claim("oid", user.EntraObjectId)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
