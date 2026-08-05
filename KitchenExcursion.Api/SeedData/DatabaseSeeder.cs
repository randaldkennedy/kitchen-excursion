using System.Text.Json;
using KitchenExcursion.Api.Data;
using KitchenExcursion.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KitchenExcursion.Api.SeedData;

public static class DatabaseSeeder
{
    public static async Task SeedRecipesAsync(
        KitchenExcursionContext context,
        IWebHostEnvironment environment)
    {
        if (await context.Recipes.AnyAsync())
        {
            return;
        }

        var seedFilePath = Path.Combine(
            environment.ContentRootPath,
            "SeedData",
            "recipes.json"
        );

        if (!File.Exists(seedFilePath))
        {
            throw new FileNotFoundException(
                "The recipe seed file was not found.",
                seedFilePath
            );
        }

        var json = await File.ReadAllTextAsync(seedFilePath);

        var seedRecipes = JsonSerializer.Deserialize<List<SeedRecipe>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        if (seedRecipes is null || seedRecipes.Count == 0)
        {
            return;
        }

        var recipes = seedRecipes.Select(seed => new Recipe
        {
            Slug = seed.Slug,
            Title = seed.Title,
            Summary = seed.Summary,
            Badge = seed.Badge,
            Image = seed.Image,
            ImageAlt = seed.ImageAlt,
            PrepTime = seed.PrepTime,
            CookTime = seed.CookTime,
            Serves = seed.Serves
        });

        await context.Recipes.AddRangeAsync(recipes);
        await context.SaveChangesAsync();
    }
}