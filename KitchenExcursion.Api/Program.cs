using KitchenExcursion.Api.Data;
using KitchenExcursion.Api.Endpoints;
using KitchenExcursion.Api.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddAuthentication(DevelopmentAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
            DevelopmentAuthenticationHandler.SchemeName,
            _ => { });
}
else
{
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    builder.Services.Configure<OpenIdConnectOptions>(
        OpenIdConnectDefaults.AuthenticationScheme,
        options =>
        {
            options.SignedOutRedirectUri = "https://laultimaexcursion.com";
        });
}

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<KitchenExcursionContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("KitchenExcursion"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(15),
                errorNumbersToAdd: null);
        }));

builder.Services.AddDbContext<LaUltimaExcursionDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("KitchenExcursion"),
        sqlOptions =>
        {
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "platform");
            sqlOptions.EnableRetryOnFailure();
        }));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();

app.MapAuthEndpoints();
app.MapRecipeEndpoints();

app.MapGet("/api/version", async (IWebHostEnvironment environment) =>
{
    var buildInfoPath = Path.Combine(
        environment.ContentRootPath,
        "build-info.json");

    if (File.Exists(buildInfoPath))
    {
        var json = await File.ReadAllTextAsync(buildInfoPath);
        return Results.Text(json, "application/json");
    }

    return Results.Ok(new
    {
        app = "Kitchen Excursion",
        version = "0.6.0",
        commit = "local",
        builtAt = DateTime.UtcNow
    });
});

app.Run();
