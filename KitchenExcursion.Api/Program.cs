using System.Text.Json;
using System.Text.Json.Nodes;
using KitchenExcursion.Api.Data;
using Microsoft.EntityFrameworkCore;
using KitchenExcursion.Api.SeedData;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider.GetRequiredService<KitchenExcursionContext>();

    await DatabaseSeeder.SeedRecipesAsync(
        context,
        app.Environment);
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

var recipesPath = Path.Combine(
    app.Environment.ContentRootPath,
    "SeedData",
    "recipes.json"
);
/*
app.MapGet("/api/recipes", async(KitchenExcursionContext context) =>
{
    var recipes = await context.Recipes
        .AsNoTracking()
        .OrderBy(recipe => recipe.RecipeId)
        .ToListAsync();

    return Results.Ok(recipes);
});
*/

app.MapGet("/api/recipes", async () =>
{
    if (!File.Exists(recipesPath))
    {
        return Results.NotFound(new
        {
            message = "recipes.json was not found.",
            path = recipesPath
        });
    }

    var json = await File.ReadAllTextAsync(recipesPath);

    return Results.Text(json, "application/json");
});

app.MapPost(
    "/api/recipes/{id}/cook-log",
    async (string id, CookLogRequest request) =>
    {
        if (string.IsNullOrWhiteSpace(request.Note))
        {
            return Results.BadRequest(new
            {
                message = "A cook-log note is required."
            });
        }

        var json = await File.ReadAllTextAsync(recipesPath);
        var recipes = JsonNode.Parse(json)?.AsArray();

        if (recipes is null)
        {
            return Results.Problem(
                "recipes.json does not contain a valid recipe array."
            );
        }

        var recipe = recipes.FirstOrDefault(item =>
            string.Equals(
                item?["id"]?.ToString(),
                id,
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (recipe is null)
        {
            return Results.NotFound(new
            {
                message = $"Recipe '{id}' was not found."
            });
        }

        var journal = recipe["journal"] as JsonObject;

        if (journal is null)
        {
            journal = new JsonObject();
            recipe["journal"] = journal;
        }

        var cookLog = journal["cookLog"] as JsonArray;

        if (cookLog is null)
        {
            cookLog = new JsonArray();
            journal["cookLog"] = cookLog;
        }

        var entry = new JsonObject
        {
            ["date"] = DateTimeOffset.Now.ToString("O"),
            ["author"] = string.IsNullOrWhiteSpace(request.Author)
                ? "Randy"
                : request.Author.Trim(),
            ["note"] = request.Note.Trim()
        };

        cookLog.Insert(0, entry);

        await File.WriteAllTextAsync(
            recipesPath,
            recipes.ToJsonString(
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            )
        );

        return Results.Ok(entry);
    }
);

app.MapGet("/api/version", async (IWebHostEnvironment environment) =>
{
    var buildInfoPath = Path.Combine(
        environment.ContentRootPath,
        "build-info.json"
    );

    if (File.Exists(buildInfoPath))
    {
        var json = await File.ReadAllTextAsync(buildInfoPath);
        return Results.Text(json, "application/json");
    }

    return Results.Ok(new
    {
        app = "Kitchen Excursion",
        version = "0.3.1",
        commit = "local",
        builtAt = DateTime.UtcNow
    });
});

app.Run();

record CookLogRequest(string? Author, string Note);