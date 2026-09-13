using KitchenExcursion.Api.Data;
using KitchenExcursion.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KitchenExcursion.Api.Endpoints;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/recipes", GetRecipesAsync);
        app.MapGet("/api/recipes/{id}", GetRecipeAsync);

        app.MapPost("/api/recipes", CreateRecipeAsync);
        app.MapPut("/api/recipes/{id}", UpdateRecipeAsync);
        app.MapDelete("/api/recipes/{id}", DeleteRecipeAsync);

        app.MapPost("/api/recipes/{id}/cook-log", CreateCookLogAsync);
        app.MapPut("/api/recipes/{id}/cook-log/{cookLogId:long}", UpdateCookLogAsync);
        app.MapDelete("/api/recipes/{id}/cook-log/{cookLogId:long}", DeleteCookLogAsync);

        app.MapPut("/api/recipes/{id}/cook-log/{cookLogId:long}/ratings/{rater}", UpsertCookRatingAsync);
        app.MapDelete("/api/recipes/{id}/cook-log/{cookLogId:long}/ratings/{rater}", DeleteCookRatingAsync);

        return app;
    }

    private static async Task<IResult> GetRecipesAsync(
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        var recipes = await RecipeQuery(db)
            .OrderBy(r => r.RecipeId)
            .ToListAsync(cancellationToken);

        return Results.Ok(recipes.Select(ToRecipeDto));
    }

    private static async Task<IResult> GetRecipeAsync(
        string id,
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        var recipe = await RecipeQuery(db)
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        return recipe is null
            ? Results.NotFound(new { message = $"Recipe '{id}' was not found." })
            : Results.Ok(ToRecipeDto(recipe));
    }

    private static async Task<IResult> CreateRecipeAsync(
        RecipeWriteRequest request,
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        var validation = ValidateRecipeRequest(request);
        if (validation is not null)
        {
            return Results.BadRequest(new { message = validation });
        }

        var slug = request.Id.Trim();
        var exists = await db.Recipes
            .AnyAsync(r => r.Slug == slug, cancellationToken);

        if (exists)
        {
            return Results.Conflict(new
            {
                message = $"A recipe with id '{slug}' already exists."
            });
        }

        var recipe = new Recipe
        {
            Slug = slug,
            Title = request.Title.Trim()
        };

        ApplyRecipeRequest(recipe, request);
        db.Recipes.Add(recipe);

        await db.SaveChangesAsync(cancellationToken);

        var created = await RecipeQuery(db)
            .SingleAsync(r => r.RecipeId == recipe.RecipeId, cancellationToken);

        return Results.Created(
            $"/api/recipes/{created.Slug}",
            ToRecipeDto(created));
    }

    private static async Task<IResult> UpdateRecipeAsync(
        string id,
        RecipeWriteRequest request,
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        var validation = ValidateRecipeRequest(request);
        if (validation is not null)
        {
            return Results.BadRequest(new { message = validation });
        }

        var recipe = await db.Recipes
            .Include(r => r.Categories)
            .Include(r => r.Statuses)
            .Include(r => r.Ingredients)
            .Include(r => r.Steps)
            .Include(r => r.ShoppingItems)
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
        {
            return Results.NotFound(new
            {
                message = $"Recipe '{id}' was not found."
            });
        }

        var requestedSlug = request.Id.Trim();
        if (!string.Equals(id, requestedSlug, StringComparison.OrdinalIgnoreCase))
        {
            var slugExists = await db.Recipes.AnyAsync(
                r => r.RecipeId != recipe.RecipeId && r.Slug == requestedSlug,
                cancellationToken);

            if (slugExists)
            {
                return Results.Conflict(new
                {
                    message = $"A recipe with id '{requestedSlug}' already exists."
                });
            }

            recipe.Slug = requestedSlug;
        }

        ReplaceRecipeChildren(db, recipe);
        ApplyRecipeRequest(recipe, request);

        await db.SaveChangesAsync(cancellationToken);

        var updated = await RecipeQuery(db)
            .SingleAsync(r => r.RecipeId == recipe.RecipeId, cancellationToken);

        return Results.Ok(ToRecipeDto(updated));
    }

    private static async Task<IResult> DeleteRecipeAsync(
        string id,
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        var recipe = await db.Recipes
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
        {
            return Results.NotFound(new
            {
                message = $"Recipe '{id}' was not found."
            });
        }

        db.Recipes.Remove(recipe);
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> CreateCookLogAsync(
        string id,
        CookLogWriteRequest request,
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
        {
            return Results.BadRequest(new
            {
                message = "A cook-log note is required."
            });
        }

        var recipe = await db.Recipes
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
        {
            return Results.NotFound(new
            {
                message = $"Recipe '{id}' was not found."
            });
        }

        var entry = new RecipeCookLog
        {
            RecipeId = recipe.RecipeId,
            CookedAt = request.Date ?? DateTimeOffset.Now,
            Author = NormalizeAuthor(request.Author),
            Note = request.Note.Trim()
        };

        foreach (var rating in NormalizeRatings(request.Ratings))
        {
            entry.Ratings.Add(new RecipeCookRating
            {
                Rater = rating.Rater,
                Stars = rating.Stars
            });
        }

        db.RecipeCookLogs.Add(entry);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/recipes/{id}/cook-log/{entry.Id}",
            ToCookLogDto(entry));
    }

    private static async Task<IResult> UpdateCookLogAsync(
        string id,
        long cookLogId,
        CookLogWriteRequest request,
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
        {
            return Results.BadRequest(new
            {
                message = "A cook-log note is required."
            });
        }

        var entry = await db.RecipeCookLogs
            .Include(c => c.Recipe)
            .Include(c => c.Ratings)
            .SingleOrDefaultAsync(
                c => c.Id == cookLogId && c.Recipe.Slug == id,
                cancellationToken);

        if (entry is null)
        {
            return Results.NotFound(new
            {
                message = $"Cook log '{cookLogId}' was not found for recipe '{id}'."
            });
        }

        entry.CookedAt = request.Date ?? entry.CookedAt;
        entry.Author = NormalizeAuthor(request.Author);
        entry.Note = request.Note.Trim();

        if (request.Ratings is not null)
        {
            db.RecipeCookRatings.RemoveRange(entry.Ratings);
            entry.Ratings.Clear();

            foreach (var rating in NormalizeRatings(request.Ratings))
            {
                entry.Ratings.Add(new RecipeCookRating
                {
                    Rater = rating.Rater,
                    Stars = rating.Stars
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(ToCookLogDto(entry));
    }

    private static async Task<IResult> DeleteCookLogAsync(
        string id,
        long cookLogId,
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        var entry = await db.RecipeCookLogs
            .Include(c => c.Recipe)
            .SingleOrDefaultAsync(
                c => c.Id == cookLogId && c.Recipe.Slug == id,
                cancellationToken);

        if (entry is null)
        {
            return Results.NotFound(new
            {
                message = $"Cook log '{cookLogId}' was not found for recipe '{id}'."
            });
        }

        db.RecipeCookLogs.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> UpsertCookRatingAsync(
        string id,
        long cookLogId,
        string rater,
        RatingWriteRequest request,
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        if (request.Stars is < 1 or > 5)
        {
            return Results.BadRequest(new
            {
                message = "Stars must be between 1 and 5."
            });
        }

        var cleanRater = Uri.UnescapeDataString(rater).Trim();
        if (string.IsNullOrWhiteSpace(cleanRater))
        {
            return Results.BadRequest(new
            {
                message = "A rater name is required."
            });
        }

        var entry = await db.RecipeCookLogs
            .Include(c => c.Recipe)
            .Include(c => c.Ratings)
            .SingleOrDefaultAsync(
                c => c.Id == cookLogId && c.Recipe.Slug == id,
                cancellationToken);

        if (entry is null)
        {
            return Results.NotFound(new
            {
                message = $"Cook log '{cookLogId}' was not found for recipe '{id}'."
            });
        }

        var rating = entry.Ratings.FirstOrDefault(r =>
            string.Equals(r.Rater, cleanRater, StringComparison.OrdinalIgnoreCase));

        if (rating is null)
        {
            rating = new RecipeCookRating
            {
                RecipeCookLogId = entry.Id,
                Rater = cleanRater,
                Stars = request.Stars
            };

            db.RecipeCookRatings.Add(rating);
            entry.Ratings.Add(rating);
        }
        else
        {
            rating.Rater = cleanRater;
            rating.Stars = request.Stars;
        }

        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(new
        {
            rater = rating.Rater,
            stars = rating.Stars
        });
    }

    private static async Task<IResult> DeleteCookRatingAsync(
        string id,
        long cookLogId,
        string rater,
        KitchenExcursionContext db,
        CancellationToken cancellationToken)
    {
        var cleanRater = Uri.UnescapeDataString(rater).Trim();

        var entry = await db.RecipeCookLogs
            .Include(c => c.Recipe)
            .Include(c => c.Ratings)
            .SingleOrDefaultAsync(
                c => c.Id == cookLogId && c.Recipe.Slug == id,
                cancellationToken);

        if (entry is null)
        {
            return Results.NotFound(new
            {
                message = $"Cook log '{cookLogId}' was not found for recipe '{id}'."
            });
        }

        var rating = entry.Ratings.FirstOrDefault(r =>
            string.Equals(r.Rater, cleanRater, StringComparison.OrdinalIgnoreCase));

        if (rating is null)
        {
            return Results.NotFound(new
            {
                message = $"Rating for '{cleanRater}' was not found on cook log '{cookLogId}'."
            });
        }

        db.RecipeCookRatings.Remove(rating);
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static IQueryable<Recipe> RecipeQuery(KitchenExcursionContext db)
    {
        return db.Recipes
            .AsNoTracking()
            .Include(r => r.Categories)
            .Include(r => r.Statuses)
            .Include(r => r.Ingredients)
            .Include(r => r.Steps)
            .Include(r => r.ShoppingItems)
            .Include(r => r.CookLogs)
                .ThenInclude(c => c.Ratings)
            .AsSplitQuery();
    }

    private static string? ValidateRecipeRequest(RecipeWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            return "Recipe id is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return "Recipe title is required.";
        }

        if (request.Id.Trim().Length > 160)
        {
            return "Recipe id cannot exceed 160 characters.";
        }

        if (request.Title.Trim().Length > 240)
        {
            return "Recipe title cannot exceed 240 characters.";
        }

        return null;
    }

    private static void ReplaceRecipeChildren(
        KitchenExcursionContext db,
        Recipe recipe)
    {
        db.RecipeCategories.RemoveRange(recipe.Categories);
        db.RecipeStatuses.RemoveRange(recipe.Statuses);
        db.RecipeIngredients.RemoveRange(recipe.Ingredients);
        db.RecipeSteps.RemoveRange(recipe.Steps);
        db.RecipeShoppingItems.RemoveRange(recipe.ShoppingItems);

        recipe.Categories.Clear();
        recipe.Statuses.Clear();
        recipe.Ingredients.Clear();
        recipe.Steps.Clear();
        recipe.ShoppingItems.Clear();
    }

    private static void ApplyRecipeRequest(
        Recipe recipe,
        RecipeWriteRequest request)
    {
        recipe.Title = request.Title.Trim();
        recipe.Meal = CleanOptional(request.Meal);
        recipe.Protein = CleanOptional(request.Protein);
        recipe.Method = CleanOptional(request.Method);
        recipe.Badge = CleanOptional(request.Badge);
        recipe.Image = CleanOptional(request.Image);
        recipe.ImageAlt = CleanOptional(request.ImageAlt);
        recipe.Summary = CleanOptional(request.Summary);
        recipe.PrepTime = CleanOptional(request.Prep);
        recipe.CookTime = CleanOptional(request.Cook);
        recipe.Serves = CleanOptional(request.Serves);
        recipe.GeneralNotes = CleanOptional(request.Journal?.General);

        AddCategories(recipe, request.Categories);
        AddStatuses(recipe, request.Status);
        AddIngredients(recipe, request.Ingredients);
        AddSteps(recipe, request.Steps);
        AddShoppingItems(recipe, request.Shopping);
    }

    private static void AddCategories(Recipe recipe, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            recipe.Categories.Add(new RecipeCategory
            {
                Name = items[i],
                SortOrder = i
            });
        }
    }

    private static void AddStatuses(Recipe recipe, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            recipe.Statuses.Add(new RecipeStatus
            {
                Value = items[i],
                SortOrder = i
            });
        }
    }

    private static void AddIngredients(Recipe recipe, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            recipe.Ingredients.Add(new RecipeIngredient
            {
                Text = items[i],
                SortOrder = i
            });
        }
    }

    private static void AddSteps(Recipe recipe, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            recipe.Steps.Add(new RecipeStep
            {
                Text = items[i],
                SortOrder = i
            });
        }
    }

    private static void AddShoppingItems(Recipe recipe, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            recipe.ShoppingItems.Add(new RecipeShoppingItem
            {
                Text = items[i],
                SortOrder = i
            });
        }
    }

    private static List<string> CleanList(IEnumerable<string>? values)
    {
        return values?
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .ToList()
            ?? new List<string>();
    }

    private static string? CleanOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string NormalizeAuthor(string? author)
    {
        return string.IsNullOrWhiteSpace(author)
            ? "Randy"
            : author.Trim();
    }

    private static IReadOnlyList<NormalizedRating> NormalizeRatings(
        IEnumerable<RatingInput>? ratings)
    {
        if (ratings is null)
        {
            return Array.Empty<NormalizedRating>();
        }

        var normalized = new Dictionary<string, NormalizedRating>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var rating in ratings)
        {
            var rater = rating.Rater?.Trim();
            if (string.IsNullOrWhiteSpace(rater))
            {
                continue;
            }

            if (rating.Stars is < 1 or > 5)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ratings),
                    "Every rating must be between 1 and 5 stars.");
            }

            normalized[rater] = new NormalizedRating(rater, rating.Stars);
        }

        return normalized.Values.ToList();
    }

    private static object ToRecipeDto(Recipe recipe)
    {
        var ratings = recipe.CookLogs
            .SelectMany(c => c.Ratings)
            .Select(r => r.Stars)
            .ToList();

        return new
        {
            id = recipe.Slug,
            title = recipe.Title,
            categories = recipe.Categories
                .OrderBy(c => c.SortOrder)
                .Select(c => c.Name)
                .ToArray(),
            meal = recipe.Meal,
            protein = recipe.Protein,
            method = recipe.Method,
            status = recipe.Statuses
                .OrderBy(s => s.SortOrder)
                .Select(s => s.Value)
                .ToArray(),
            badge = recipe.Badge,
            image = recipe.Image,
            imageAlt = recipe.ImageAlt,
            summary = recipe.Summary,
            prep = recipe.PrepTime,
            cook = recipe.CookTime,
            serves = recipe.Serves,
            ingredients = recipe.Ingredients
                .OrderBy(i => i.SortOrder)
                .Select(i => i.Text)
                .ToArray(),
            steps = recipe.Steps
                .OrderBy(s => s.SortOrder)
                .Select(s => s.Text)
                .ToArray(),
            journal = new
            {
                general = recipe.GeneralNotes,
                cookLog = recipe.CookLogs
                    .OrderByDescending(c => c.CookedAt)
                    .ThenByDescending(c => c.Id)
                    .Select(ToCookLogDto)
                    .ToArray()
            },
            shopping = recipe.ShoppingItems
                .OrderBy(i => i.SortOrder)
                .Select(i => i.Text)
                .ToArray(),
            rating = ratings.Count == 0
                ? (double?)null
                : Math.Round(ratings.Average(), 1)
        };
    }

    private static object ToCookLogDto(RecipeCookLog entry)
    {
        return new
        {
            id = entry.Id,
            date = entry.CookedAt,
            author = entry.Author,
            note = entry.Note,
            ratings = entry.Ratings
                .OrderBy(r => r.Rater)
                .Select(r => new
                {
                    rater = r.Rater,
                    stars = r.Stars
                })
                .ToArray()
        };
    }

    public sealed record RecipeWriteRequest(
        string Id,
        string Title,
        string[]? Categories,
        string? Meal,
        string? Protein,
        string? Method,
        string[]? Status,
        string? Badge,
        string? Image,
        string? ImageAlt,
        string? Summary,
        string? Prep,
        string? Cook,
        string? Serves,
        string[]? Ingredients,
        string[]? Steps,
        JournalWriteRequest? Journal,
        string[]? Shopping);

    public sealed record JournalWriteRequest(string? General);

    public sealed record CookLogWriteRequest(
        string? Author,
        string Note,
        DateTimeOffset? Date,
        RatingInput[]? Ratings);

    public sealed record RatingInput(string? Rater, int Stars);

    public sealed record RatingWriteRequest(int Stars);

    private sealed record NormalizedRating(string Rater, int Stars);
}
