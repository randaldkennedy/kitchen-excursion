using KitchenExcursion.Api.Data;
using KitchenExcursion.Api.Models;
using KitchenExcursion.Api.Services.Attachments;
using Microsoft.EntityFrameworkCore;

namespace KitchenExcursion.Api.Endpoints;

public static class RecipePhotoEndpoints
{
    private const string EntraObjectIdClaim =
        "http://schemas.microsoft.com/identity/claims/objectidentifier";

    private const string AppName = "kitchen";
    private const string Category = "recipe-hero";
    private const string EntityType = "recipe";
    private const long MaxPhotoBytes = 15 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public static IEndpointRouteBuilder MapRecipePhotoEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/recipes/{id}/hero-photo", GetHeroPhotoAsync)
            .AllowAnonymous();

        app.MapPost("/api/recipes/{id}/hero-photo", UploadHeroPhotoAsync)
            .DisableAntiforgery()
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetHeroPhotoAsync(
        string id,
        HttpContext httpContext,
        KitchenExcursionContext kitchenDb,
        LaUltimaExcursionDbContext platformDb,
        IAttachmentStorageService storage,
        CancellationToken cancellationToken)
    {
        var recipe = await kitchenDb.Recipes
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
            return Results.NotFound();

        var attachment = await platformDb.Attachments
            .AsNoTracking()
            .Where(a =>
                a.HouseholdId == recipe.HouseholdId &&
                a.App == AppName &&
                a.Category == Category &&
                a.EntityType == EntityType &&
                a.EntityId == id &&
                a.IsActive)
            .OrderByDescending(a => a.UploadedUtc)
            .ThenByDescending(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (attachment is null)
            return Results.NotFound();

        var download = await storage.OpenReadAsync(
            attachment.BlobName,
            cancellationToken);

        if (download is null)
            return Results.NotFound();

        httpContext.Response.Headers.CacheControl = "public, max-age=300";

        return Results.File(
            download.Content,
            attachment.ContentType,
            enableRangeProcessing: true);
    }

    private static async Task<IResult> UploadHeroPhotoAsync(
        string id,
        IFormFile file,
        HttpContext httpContext,
        KitchenExcursionContext kitchenDb,
        LaUltimaExcursionDbContext platformDb,
        IAttachmentStorageService storage,
        CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
            return Results.BadRequest(new { message = "Choose an image to upload." });

        if (file.Length > MaxPhotoBytes)
            return Results.BadRequest(new { message = "Recipe photos must be 15 MB or smaller." });

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return Results.BadRequest(new
            {
                message = "Recipe photos must be JPG, PNG, or WebP images."
            });
        }

        var recipe = await kitchenDb.Recipes
            .SingleOrDefaultAsync(r => r.Slug == id, cancellationToken);

        if (recipe is null)
            return Results.NotFound(new { message = $"Recipe '{id}' was not found." });

        var currentUser = await GetCurrentUserAsync(
            httpContext,
            platformDb,
            cancellationToken);

        if (currentUser is null)
            return Results.Unauthorized();

        var belongsToRecipeHousehold = await platformDb.HouseholdMembers
            .AnyAsync(
                hm => hm.HouseholdId == recipe.HouseholdId &&
                      hm.UserId == currentUser.Id,
                cancellationToken);

        if (!belongsToRecipeHousehold)
            return Results.Forbid();

        var previous = await platformDb.Attachments
            .Where(a =>
                a.HouseholdId == recipe.HouseholdId &&
                a.App == AppName &&
                a.Category == Category &&
                a.EntityType == EntityType &&
                a.EntityId == recipe.Slug &&
                a.IsActive)
            .OrderByDescending(a => a.UploadedUtc)
            .ThenByDescending(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        await using var content = file.OpenReadStream();

        var stored = await storage.UploadAsync(
            recipe.HouseholdId,
            AppName,
            Category,
            file.FileName,
            file.ContentType,
            content,
            cancellationToken);

        var attachment = new Attachment
        {
            HouseholdId = recipe.HouseholdId,
            UploadedByUserId = currentUser.Id,
            App = AppName,
            Category = Category,
            EntityType = EntityType,
            EntityId = recipe.Slug,
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            BlobName = stored.BlobName,
            FileSizeBytes = stored.FileSizeBytes,
            UploadedUtc = DateTime.UtcNow,
            EffectiveDate = DateTime.UtcNow,
            IsActive = true,
            SupersedesId = previous?.Id
        };

        if (previous is not null)
            previous.IsActive = false;

        platformDb.Attachments.Add(attachment);

        recipe.Image = $"/api/recipes/{Uri.EscapeDataString(recipe.Slug)}/hero-photo";

        await platformDb.SaveChangesAsync(cancellationToken);
        await kitchenDb.SaveChangesAsync(cancellationToken);

        return Results.Ok(new
        {
            attachmentId = attachment.Id,
            image = recipe.Image,
            fileName = attachment.FileName,
            contentType = attachment.ContentType,
            fileSizeBytes = attachment.FileSizeBytes
        });
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
            .SingleOrDefaultAsync(
                u => u.EntraObjectId == entraObjectId,
                cancellationToken);
    }
}
