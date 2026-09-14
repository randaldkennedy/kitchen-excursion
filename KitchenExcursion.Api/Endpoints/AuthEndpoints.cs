using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace KitchenExcursion.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/status", (HttpContext httpContext) =>
        {
            return Results.Ok(new
            {
                isAuthenticated =
                    httpContext.User.Identity?.IsAuthenticated ?? false
            });
        })
        .AllowAnonymous();

        app.MapGet("/api/auth/login", () =>
        {
            return Results.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = "/"
                },
                new[] { OpenIdConnectDefaults.AuthenticationScheme });
        })
        .AllowAnonymous();

        app.MapGet("/api/auth/me", (HttpContext httpContext) =>
        {
            var principal = httpContext.User;

            var entraObjectId =
                principal.FindFirst(
                    "http://schemas.microsoft.com/identity/claims/objectidentifier"
                )?.Value
                ?? principal.FindFirst("oid")?.Value;

            var email =
                principal.FindFirst("emails")?.Value
                ?? principal.FindFirst("preferred_username")?.Value
                ?? principal.FindFirst(
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
                )?.Value
                ?? principal.Identity?.Name;

            var givenName =
                principal.FindFirst(
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"
                )?.Value
                ?? principal.FindFirst("given_name")?.Value;

            return Results.Ok(new
            {
                isAuthenticated = true,
                entraObjectId,
                email,
                givenName
            });
        })
        .RequireAuthorization();

        app.MapGet("/api/auth/logout", async (
            HttpContext httpContext,
            IWebHostEnvironment environment) =>
        {
            if (environment.IsDevelopment())
            {
                return Results.Redirect("/");
            }

            await httpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            await httpContext.SignOutAsync(
                OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = "https://laultimaexcursion.com"
                });

            return Results.Empty;
        })
        .RequireAuthorization();

        return app;
    }
}
