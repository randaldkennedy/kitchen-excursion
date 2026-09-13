using System.Text.Json;
using KitchenExcursion.Api.Data;
using KitchenExcursion.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KitchenExcursion.Api.SeedData;

public static class DatabaseSeeder
{
    /// <summary>
    /// One-way importer for the legacy recipes.json data. The JSON file remains
    /// in the repository as a safety net, but SQL becomes the source of truth
    /// after each recipe has normalized child rows.
    /// </summary>
    public static async Task ImportLegacyRecipesAsync(
        KitchenExcursionContext context,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        var seedFilePath = Path.Combine(
            environment.ContentRootPath,
            "SeedData",
            "recipes.json");

        if (!File.Exists(seedFilePath))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(seedFilePath, cancellationToken);
        var seedRecipes = JsonSerializer.Deserialize<List<SeedRecipe>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (seedRecipes is null || seedRecipes.Count == 0)
        {
            return;
        }

        foreach (var seed in seedRecipes)
        {
            var recipe = await context.Recipes
                .Include(r => r.Categories)
                .Include(r => r.Statuses)
                .Include(r => r.Ingredients)
                .Include(r => r.Steps)
                .Include(r => r.ShoppingItems)
                .Include(r => r.CookLogs)
                .SingleOrDefaultAsync(
                    r => r.Slug == seed.Slug,
                    cancellationToken);

            var isNewRecipe = recipe is null;

            if (recipe is null)
            {
                recipe = new Recipe
                {
                    Slug = seed.Slug,
                    Title = seed.Title
                };

                context.Recipes.Add(recipe);
            }

            var needsLegacyImport = isNewRecipe ||
                (recipe.Categories.Count == 0 &&
                 recipe.Statuses.Count == 0 &&
                 recipe.Ingredients.Count == 0 &&
                 recipe.Steps.Count == 0 &&
                 recipe.ShoppingItems.Count == 0 &&
                 recipe.CookLogs.Count == 0);

            if (!needsLegacyImport)
            {
                continue;
            }

            recipe.Title = seed.Title;
            recipe.Summary = seed.Summary;
            recipe.Badge = seed.Badge;
            recipe.Image = seed.Image;
            recipe.ImageAlt = seed.ImageAlt;
            recipe.PrepTime = seed.PrepTime;
            recipe.CookTime = seed.CookTime;
            recipe.Serves = seed.Serves;
            recipe.Meal = seed.Meal;
            recipe.Protein = seed.Protein;
            recipe.Method = seed.Method;
            recipe.GeneralNotes = seed.Journal?.General;

            // Only import each normalized collection when it is empty. That
            // makes startup safe after SQL becomes editable: stale JSON will
            // never overwrite later database changes.
            if (recipe.Categories.Count == 0)
            {
                AddOrdered(
                    seed.Categories,
                    recipe.Categories,
                    (value, order) => new RecipeCategory
                    {
                        Name = value,
                        SortOrder = order
                    });
            }

            if (recipe.Statuses.Count == 0)
            {
                AddOrdered(
                    seed.Status,
                    recipe.Statuses,
                    (value, order) => new RecipeStatus
                    {
                        Value = value,
                        SortOrder = order
                    });
            }

            if (recipe.Ingredients.Count == 0)
            {
                AddOrdered(
                    seed.Ingredients,
                    recipe.Ingredients,
                    (value, order) => new RecipeIngredient
                    {
                        Text = value,
                        SortOrder = order
                    });
            }

            if (recipe.Steps.Count == 0)
            {
                AddOrdered(
                    seed.Steps,
                    recipe.Steps,
                    (value, order) => new RecipeStep
                    {
                        Text = value,
                        SortOrder = order
                    });
            }

            if (recipe.ShoppingItems.Count == 0)
            {
                AddOrdered(
                    seed.Shopping,
                    recipe.ShoppingItems,
                    (value, order) => new RecipeShoppingItem
                    {
                        Text = value,
                        SortOrder = order
                    });
            }

            if (recipe.CookLogs.Count == 0 && seed.Journal?.CookLog.Count > 0)
            {
                foreach (var entry in seed.Journal.CookLog)
                {
                    recipe.CookLogs.Add(new RecipeCookLog
                    {
                        CookedAt = entry.Date,
                        Author = string.IsNullOrWhiteSpace(entry.Author)
                            ? "Randy"
                            : entry.Author.Trim(),
                        Note = entry.Note?.Trim() ?? string.Empty
                    });
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void AddOrdered<T>(
        IReadOnlyList<string> values,
        ICollection<T> destination,
        Func<string, int, T> factory)
    {
        for (var index = 0; index < values.Count; index++)
        {
            destination.Add(factory(values[index], index));
        }
    }
}
