using KitchenExcursion.Api.Data;
using KitchenExcursion.Api.Models;
using KitchenExcursion.Api.Services.Attachments;
using Microsoft.EntityFrameworkCore;

namespace KitchenExcursion.Api.Endpoints;

public static class RecipeEndpoints
{
    private const string EntraObjectIdClaim =
        "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public static IEndpointRouteBuilder MapRecipeEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/recipes", GetRecipesAsync)
            .AllowAnonymous();

        app.MapGet("/api/recipes/{id}", GetRecipeAsync)
            .AllowAnonymous();

        app.MapGet("/api/recipes/{id}/revisions", GetRecipeRevisionsAsync)
            .AllowAnonymous();

        app.MapPost("/api/recipes", CreateRecipeAsync)
            .RequireAuthorization();

        app.MapPut("/api/recipes/{id}", UpdateRecipeAsync)
            .RequireAuthorization();

        app.MapDelete("/api/recipes/{id}", DeleteRecipeAsync)
            .RequireAuthorization();

        app.MapPost("/api/recipes/{id}/cook-log", CreateCookLogAsync)
            .RequireAuthorization();

        app.MapPut("/api/recipes/{id}/cook-log/{cookLogId:long}", UpdateCookLogAsync)
            .RequireAuthorization();

        app.MapDelete("/api/recipes/{id}/cook-log/{cookLogId:long}", DeleteCookLogAsync)
            .RequireAuthorization();

        app.MapPut("/api/recipes/{id}/cook-log/{cookLogId:long}/ratings/{rater}", UpsertCookRatingAsync)
            .RequireAuthorization();

        app.MapDelete("/api/recipes/{id}/cook-log/{cookLogId:long}/ratings/{rater}", DeleteCookRatingAsync)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetRecipesAsync(
        HttpContext httpContext,
        KitchenExcursionContext db,
        LaUltimaExcursionDbContext platformDb,
        CancellationToken cancellationToken)
    {
        var recipes = await RecipeQuery(db)
            .OrderBy(r => r.RecipeId)
            .ToListAsync(cancellationToken);

        var editableHouseholds = await GetEditableHouseholdIdsAsync(
            httpContext,
            platformDb,
            cancellationToken);

        return Results.Ok(recipes.Select(r =>
            ToRecipeDto(r, editableHouseholds.Contains(r.HouseholdId))));
    }

    private static async Task<IResult> GetRecipeAsync(
        string id,
        HttpContext httpContext,
        KitchenExcursionContext db,
        LaUltimaExcursionDbContext platformDb,
        CancellationToken cancellationToken)
    {
        var recipe = await RecipeQuery(db)
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
            return Results.NotFound(new { message = $"Recipe '{id}' was not found." });

        var editableHouseholds = await GetEditableHouseholdIdsAsync(
            httpContext,
            platformDb,
            cancellationToken);

        return Results.Ok(
            ToRecipeDto(recipe, editableHouseholds.Contains(recipe.HouseholdId)));
    }

    private static async Task<IResult> GetRecipeRevisionsAsync(
        string id,
        KitchenExcursionContext db,
        LaUltimaExcursionDbContext platformDb,
        CancellationToken cancellationToken)
    {
        var recipe = await db.Recipes
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
            return Results.NotFound(new { message = $"Recipe '{id}' was not found." });

        var revisions = await db.RecipeRevisions
            .AsNoTracking()
            .Where(r => r.RecipeId == recipe.RecipeId)
            .OrderByDescending(r => r.RevisionNumber)
            .Select(r => new
            {
                r.RevisionNumber,
                r.CreatedByUserId,
                r.CreatedUtc,
                r.ChangeNote
            })
            .ToListAsync(cancellationToken);

        var userIds = revisions
            .Select(r => r.CreatedByUserId)
            .Distinct()
            .ToArray();

        var users = await platformDb.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(
                u => u.Id,
                u => string.IsNullOrWhiteSpace(u.GivenName)
                    ? u.Email
                    : u.GivenName!,
                cancellationToken);

        return Results.Ok(revisions.Select(r => new
        {
            revisionNumber = r.RevisionNumber,
            createdUtc = r.CreatedUtc,
            createdBy = users.GetValueOrDefault(r.CreatedByUserId, "Unknown"),
            changeNote = r.ChangeNote
        }));
    }

    private static async Task<IResult> CreateRecipeAsync(
        RecipeWriteRequest request,
        HttpContext httpContext,
        KitchenExcursionContext db,
        LaUltimaExcursionDbContext platformDb,
        CancellationToken cancellationToken)
    {
        var validation = ValidateRecipeRequest(request);
        if (validation is not null)
            return Results.BadRequest(new { message = validation });

        var currentUser = await GetCurrentUserAsync(
            httpContext,
            platformDb,
            cancellationToken);

        if (currentUser is null || currentUser.DefaultHouseholdId is null)
            return Results.Unauthorized();

        var householdId = currentUser.DefaultHouseholdId.Value;

        var belongsToHousehold = await platformDb.HouseholdMembers
            .AnyAsync(
                hm => hm.HouseholdId == householdId && hm.UserId == currentUser.Id,
                cancellationToken);

        if (!belongsToHousehold)
            return Results.Forbid();

        var slug = request.Id.Trim();

        if (await db.Recipes.AnyAsync(r => r.Slug == slug, cancellationToken))
        {
            return Results.Conflict(new
            {
                message = $"A recipe with id '{slug}' already exists."
            });
        }

        Recipe? recipe = null;

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await db.Database.BeginTransactionAsync(cancellationToken);

            recipe = new Recipe
            {
                HouseholdId = householdId,
                CreatedByUserId = currentUser.Id,
                CreatedUtc = DateTimeOffset.UtcNow,
                Slug = slug
            };

            db.Recipes.Add(recipe);
            await db.SaveChangesAsync(cancellationToken);

            var revision = BuildRevision(
                recipe.RecipeId,
                revisionNumber: 1,
                currentUser.Id,
                request);

            db.RecipeRevisions.Add(revision);
            await db.SaveChangesAsync(cancellationToken);

            recipe.CurrentRevisionId = revision.RecipeRevisionId;
            await db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });

        var created = await RecipeQuery(db)
            .SingleAsync(r => r.RecipeId == recipe.RecipeId, cancellationToken);

        return Results.Created(
            $"/api/recipes/{created.Slug}",
            ToRecipeDto(created, canEdit: true));
    }

    private static async Task<IResult> UpdateRecipeAsync(
        string id,
        RecipeWriteRequest request,
        HttpContext httpContext,
        KitchenExcursionContext db,
        LaUltimaExcursionDbContext platformDb,
        CancellationToken cancellationToken)
    {
        var validation = ValidateRecipeRequest(request);
        if (validation is not null)
            return Results.BadRequest(new { message = validation });

        var currentUser = await GetCurrentUserAsync(
            httpContext,
            platformDb,
            cancellationToken);

        if (currentUser is null)
            return Results.Unauthorized();

        var recipe = await db.Recipes
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
            return Results.NotFound(new { message = $"Recipe '{id}' was not found." });

        var canEdit = await platformDb.HouseholdMembers
            .AnyAsync(
                hm => hm.HouseholdId == recipe.HouseholdId && hm.UserId == currentUser.Id,
                cancellationToken);

        if (!canEdit)
            return Results.Forbid();

        var requestedSlug = request.Id.Trim();
        if (!string.Equals(recipe.Slug, requestedSlug, StringComparison.OrdinalIgnoreCase))
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
        }

        var nextRevisionNumber = await db.RecipeRevisions
            .Where(r => r.RecipeId == recipe.RecipeId)
            .MaxAsync(r => r.RevisionNumber, cancellationToken) + 1;

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await db.Database.BeginTransactionAsync(cancellationToken);

            recipe.Slug = requestedSlug;

            var revision = BuildRevision(
                recipe.RecipeId,
                nextRevisionNumber,
                currentUser.Id,
                request);

            db.RecipeRevisions.Add(revision);
            await db.SaveChangesAsync(cancellationToken);

            recipe.CurrentRevisionId = revision.RecipeRevisionId;
            await db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });

        var updated = await RecipeQuery(db)
            .SingleAsync(r => r.RecipeId == recipe.RecipeId, cancellationToken);

        return Results.Ok(ToRecipeDto(updated, canEdit: true));
    }

    private static async Task<IResult> DeleteRecipeAsync(
        string id,
        HttpContext httpContext,
        KitchenExcursionContext db,
        LaUltimaExcursionDbContext platformDb,
        IAttachmentStorageService storage,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(
            httpContext,
            platformDb,
            cancellationToken);

        if (currentUser is null)
            return Results.Unauthorized();

        var recipe = await db.Recipes
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
            return Results.NotFound(new { message = $"Recipe '{id}' was not found." });

        var canEdit = await platformDb.HouseholdMembers
            .AnyAsync(
                hm => hm.HouseholdId == recipe.HouseholdId &&
                      hm.UserId == currentUser.Id,
                cancellationToken);

        if (!canEdit)
            return Results.Forbid();

        // Capture recipe-owned attachments before deleting the recipe identity.
        // Cook-log photos use their cook-log id as EntityId, so collect those ids too.
        var cookLogEntityIds = await db.RecipeCookLogs
            .AsNoTracking()
            .Where(c => c.RecipeId == recipe.RecipeId)
            .Select(c => c.Id.ToString())
            .ToListAsync(cancellationToken);

        var attachments = await platformDb.Attachments
            .Where(a =>
                a.HouseholdId == recipe.HouseholdId &&
                a.App == "kitchen" &&
                (
                    (a.EntityType == "recipe" &&
                     a.EntityId == recipe.Slug)
                    ||
                    (a.Category == "cook-log-photo" &&
                     a.EntityType == "recipe-cook-log" &&
                     a.EntityId != null &&
                     cookLogEntityIds.Contains(a.EntityId))
                ))
            .ToListAsync(cancellationToken);

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await db.Database.BeginTransactionAsync(cancellationToken);

            // Recipes.CurrentRevisionId points back into RecipeRevisions. Clear it
            // first so the cascade from Recipe -> Revisions can complete cleanly.
            recipe.CurrentRevisionId = null;
            await db.SaveChangesAsync(cancellationToken);

            db.Recipes.Remove(recipe);
            await db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });

        // Database deletion is authoritative. Clean up recipe-owned blobs and their
        // platform metadata afterward. A failed blob cleanup must not resurrect an
        // already-deleted recipe.
        var cleanedAttachments = new List<Attachment>();

        foreach (var attachment in attachments)
        {
            try
            {
                await storage.DeleteAsync(
                    attachment.BlobName,
                    cancellationToken);

                cleanedAttachments.Add(attachment);
            }
            catch
            {
                // Leave the metadata row behind if storage cleanup fails so there
                // is still a record of the orphaned blob for later maintenance.
            }
        }

        if (cleanedAttachments.Count > 0)
        {
            platformDb.Attachments.RemoveRange(cleanedAttachments);
            await platformDb.SaveChangesAsync(cancellationToken);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> CreateCookLogAsync(
        string id,
        CookLogWriteRequest request,
        HttpContext httpContext,
        KitchenExcursionContext db,
        LaUltimaExcursionDbContext platformDb,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
            return Results.BadRequest(new { message = "A cook-log note is required." });

        var currentUser = await GetCurrentUserAsync(
            httpContext,
            platformDb,
            cancellationToken);

        if (currentUser is null)
            return Results.Unauthorized();

        var recipe = await db.Recipes
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
            return Results.NotFound(new { message = $"Recipe '{id}' was not found." });

        var entry = new RecipeCookLog
        {
            RecipeId = recipe.RecipeId,
            CreatedByUserId = currentUser.Id,
            CookedAt = request.Date ?? DateTimeOffset.Now,
            Author = DisplayName(currentUser),
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
            return Results.BadRequest(new { message = "A cook-log note is required." });

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
        LaUltimaExcursionDbContext platformDb,
        IAttachmentStorageService storage,
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

        var entityId = cookLogId.ToString();

        var attachments = await platformDb.Attachments
            .Where(a =>
                a.HouseholdId == entry.Recipe.HouseholdId &&
                a.App == "kitchen" &&
                a.Category == "cook-log-photo" &&
                a.EntityType == "recipe-cook-log" &&
                a.EntityId == entityId)
            .ToListAsync(cancellationToken);

        db.RecipeCookLogs.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);

        var cleanedAttachments = new List<Attachment>();

        foreach (var attachment in attachments)
        {
            try
            {
                await storage.DeleteAsync(
                    attachment.BlobName,
                    cancellationToken);

                cleanedAttachments.Add(attachment);
            }
            catch
            {
                // Keep metadata if blob cleanup fails so the orphan can be reconciled later.
            }
        }

        if (cleanedAttachments.Count > 0)
        {
            platformDb.Attachments.RemoveRange(cleanedAttachments);
            await platformDb.SaveChangesAsync(cancellationToken);
        }

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
            return Results.BadRequest(new { message = "Stars must be between 1 and 5." });

        var cleanRater = Uri.UnescapeDataString(rater).Trim();
        if (string.IsNullOrWhiteSpace(cleanRater))
            return Results.BadRequest(new { message = "A rater name is required." });

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
            .Include(r => r.CurrentRevision)
                .ThenInclude(r => r!.Categories)
            .Include(r => r.CurrentRevision)
                .ThenInclude(r => r!.Statuses)
            .Include(r => r.CurrentRevision)
                .ThenInclude(r => r!.Ingredients)
            .Include(r => r.CurrentRevision)
                .ThenInclude(r => r!.Steps)
            .Include(r => r.CurrentRevision)
                .ThenInclude(r => r!.ShoppingItems)
            .Include(r => r.CookLogs)
                .ThenInclude(c => c.Ratings)
            .AsSplitQuery();
    }

    private static RecipeRevision BuildRevision(
        int recipeId,
        int revisionNumber,
        int createdByUserId,
        RecipeWriteRequest request)
    {
        var revision = new RecipeRevision
        {
            RecipeId = recipeId,
            RevisionNumber = revisionNumber,
            CreatedByUserId = createdByUserId,
            CreatedUtc = DateTimeOffset.UtcNow,
            ChangeNote = CleanOptional(request.ChangeNote),
            Title = request.Title.Trim(),
            Meal = CleanOptional(request.Meal),
            Protein = CleanOptional(request.Protein),
            Method = CleanOptional(request.Method),
            Badge = CleanOptional(request.Badge),
            ImageAlt = CleanOptional(request.ImageAlt),
            Summary = CleanOptional(request.Summary),
            PrepTime = CleanOptional(request.Prep),
            CookTime = CleanOptional(request.Cook),
            Serves = CleanOptional(request.Serves),
            GeneralNotes = CleanOptional(request.Journal?.General)
        };

        AddCategories(revision, request.Categories);
        AddStatuses(revision, request.Status);
        AddIngredients(revision, request.Ingredients);
        AddSteps(revision, request.Steps);

        // Preserve normalized shopping identities when the client/importer provides
        // them. Older/manual clients can omit Shopping and fall back to ingredients.
        AddShoppingItems(revision, request.Shopping ?? request.Ingredients);

        return revision;
    }

    private static string? ValidateRecipeRequest(RecipeWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
            return "Recipe id is required.";

        if (string.IsNullOrWhiteSpace(request.Title))
            return "Recipe title is required.";

        if (request.Id.Trim().Length > 160)
            return "Recipe id cannot exceed 160 characters.";

        if (request.Title.Trim().Length > 240)
            return "Recipe title cannot exceed 240 characters.";

        if (request.ChangeNote?.Trim().Length > 500)
            return "Change note cannot exceed 500 characters.";

        return null;
    }

    private static void AddCategories(RecipeRevision revision, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            revision.Categories.Add(new RecipeCategory
            {
                Name = items[i],
                SortOrder = i
            });
        }
    }

    private static void AddStatuses(RecipeRevision revision, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            revision.Statuses.Add(new RecipeStatus
            {
                Value = items[i],
                SortOrder = i
            });
        }
    }

    private static void AddIngredients(RecipeRevision revision, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            revision.Ingredients.Add(new RecipeIngredient
            {
                Text = StripLeadingStepNumber(items[i]),
                SortOrder = i
            });
        }
    }

    private static void AddSteps(RecipeRevision revision, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            revision.Steps.Add(new RecipeStep
            {
                Text = StripLeadingStepNumber(items[i]),
                SortOrder = i
            });
        }
    }

    private static void AddShoppingItems(RecipeRevision revision, IEnumerable<string>? values)
    {
        var items = CleanList(values);
        for (var i = 0; i < items.Count; i++)
        {
            revision.ShoppingItems.Add(new RecipeShoppingItem
            {
                Text = items[i],
                SortOrder = i
            });
        }
    }

    private static string StripLeadingStepNumber(string value)
    {
        var trimmed = value.Trim();
        var index = 0;

        while (index < trimmed.Length && char.IsDigit(trimmed[index]))
            index++;

        if (index == 0 || index >= trimmed.Length)
            return trimmed;

        var separator = trimmed[index];
        if (separator is not ('.' or ')' or '-' or ':'))
            return trimmed;

        return trimmed[(index + 1)..].TrimStart();
    }

    private static List<string> CleanList(IEnumerable<string>? values)
    {
        return values?
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .ToList()
            ?? new List<string>();
    }

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeAuthor(string? author) =>
        string.IsNullOrWhiteSpace(author) ? "Cook" : author.Trim();

    private static string DisplayName(AppUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.GivenName))
            return user.GivenName.Trim();

        if (!string.IsNullOrWhiteSpace(user.Email))
            return user.Email.Trim();

        return "Cook";
    }

    private static async Task<AppUser?> GetCurrentUserAsync(
        HttpContext httpContext,
        LaUltimaExcursionDbContext platformDb,
        CancellationToken cancellationToken)
    {
        var entraObjectId =
            httpContext.User.FindFirst(EntraObjectIdClaim)?.Value
            ?? httpContext.User.FindFirst("oid")?.Value;

        if (string.IsNullOrWhiteSpace(entraObjectId))
            return null;

        return await platformDb.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                u => u.EntraObjectId == entraObjectId,
                cancellationToken);
    }

    private static async Task<HashSet<int>> GetEditableHouseholdIdsAsync(
        HttpContext httpContext,
        LaUltimaExcursionDbContext platformDb,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(
            httpContext,
            platformDb,
            cancellationToken);

        if (currentUser is null)
            return new HashSet<int>();

        var householdIds = await platformDb.HouseholdMembers
            .AsNoTracking()
            .Where(hm => hm.UserId == currentUser.Id)
            .Select(hm => hm.HouseholdId)
            .ToListAsync(cancellationToken);

        return householdIds.ToHashSet();
    }

    private static IReadOnlyList<NormalizedRating> NormalizeRatings(
        IEnumerable<RatingInput>? ratings)
    {
        if (ratings is null)
            return Array.Empty<NormalizedRating>();

        var normalized = new Dictionary<string, NormalizedRating>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var rating in ratings)
        {
            var rater = rating.Rater?.Trim();
            if (string.IsNullOrWhiteSpace(rater))
                continue;

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

    private static object ToRecipeDto(Recipe recipe, bool canEdit)
    {
        var revision = recipe.CurrentRevision
            ?? throw new InvalidOperationException(
                $"Recipe '{recipe.Slug}' does not have a current revision.");

        var ratings = recipe.CookLogs
            .SelectMany(c => c.Ratings)
            .Select(r => r.Stars)
            .ToList();

        return new
        {
            id = recipe.Slug,
            title = revision.Title,
            categories = revision.Categories
                .OrderBy(c => c.SortOrder)
                .Select(c => c.Name)
                .ToArray(),
            meal = revision.Meal,
            protein = revision.Protein,
            method = revision.Method,
            status = revision.Statuses
                .OrderBy(s => s.SortOrder)
                .Select(s => s.Value)
                .ToArray(),
            badge = revision.Badge,
            image = recipe.Image,
            imageAlt = revision.ImageAlt,
            summary = revision.Summary,
            prep = revision.PrepTime,
            cook = revision.CookTime,
            serves = revision.Serves,
            ingredients = revision.Ingredients
                .OrderBy(i => i.SortOrder)
                .Select(i => i.Text)
                .ToArray(),
            steps = revision.Steps
                .OrderBy(s => s.SortOrder)
                .Select(s => s.Text)
                .ToArray(),
            journal = new
            {
                general = revision.GeneralNotes,
                cookLog = recipe.CookLogs
                    .OrderByDescending(c => c.CookedAt)
                    .ThenByDescending(c => c.Id)
                    .Select(ToCookLogDto)
                    .ToArray()
            },
            shopping = revision.ShoppingItems
                .OrderBy(i => i.SortOrder)
                .Select(i => i.Text)
                .ToArray(),
            rating = ratings.Count == 0
                ? (double?)null
                : Math.Round(ratings.Average(), 1),
            revisionNumber = revision.RevisionNumber,
            revisionCreatedUtc = revision.CreatedUtc,
            canEdit
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
        string? ImageAlt,
        string? Summary,
        string? Prep,
        string? Cook,
        string? Serves,
        string[]? Ingredients,
        string[]? Shopping,
        string[]? Steps,
        JournalWriteRequest? Journal,
        string? ChangeNote);

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
