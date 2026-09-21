using KitchenExcursion.Api.Data;
using KitchenExcursion.Api.Models;
using KitchenExcursion.Api.Services.Attachments;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace KitchenExcursion.Api.Endpoints;

public static class RecipePhotoEndpoints
{
    private const string EntraObjectIdClaim =
        "http://schemas.microsoft.com/identity/claims/objectidentifier";

    private const string AppName = "kitchen";
    private const string HeroCategory = "recipe-hero";
    private const string SourceCategory = "recipe-source";
    private const string EntityType = "recipe";
    private const long MaxPhotoBytes = 15 * 1024 * 1024;
    private const long MaxSourceBytes = 20 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    private static readonly HashSet<string> AllowedSourceContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
            "application/pdf",
            "text/plain",
            "text/uri-list"
        };

    public static IEndpointRouteBuilder MapRecipePhotoEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/recipes/{id}/hero-photo", GetHeroPhotoAsync)
            .AllowAnonymous();

        app.MapPost("/api/recipes/{id}/hero-photo", UploadHeroPhotoAsync)
            .DisableAntiforgery()
            .RequireAuthorization();

        app.MapGet("/api/recipes/{id}/source-info", GetRecipeSourceInfoAsync)
            .AllowAnonymous();

        app.MapGet("/api/recipes/{id}/source", GetRecipeSourceAsync)
            .AllowAnonymous();

        app.MapPost("/api/recipes/{id}/source", UploadRecipeSourceAsync)
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
                a.Category == HeroCategory &&
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
                a.Category == HeroCategory &&
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
            HeroCategory,
            file.FileName,
            file.ContentType,
            content,
            cancellationToken);

        var attachment = new Attachment
        {
            HouseholdId = recipe.HouseholdId,
            UploadedByUserId = currentUser.Id,
            App = AppName,
            Category = HeroCategory,
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

    private static async Task<IResult> GetRecipeSourceInfoAsync(
        string id,
        KitchenExcursionContext kitchenDb,
        LaUltimaExcursionDbContext platformDb,
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
                a.Category == SourceCategory &&
                a.EntityType == EntityType &&
                a.EntityId == recipe.Slug &&
                a.IsActive)
            .OrderByDescending(a => a.UploadedUtc)
            .ThenByDescending(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (attachment is null)
            return Results.Ok(new { exists = false });

        var kind = attachment.ContentType.Equals(
                "text/uri-list",
                StringComparison.OrdinalIgnoreCase)
            ? "website"
            : attachment.ContentType.Equals(
                "text/plain",
                StringComparison.OrdinalIgnoreCase)
                ? "text"
                : "file";

        return Results.Ok(new
        {
            exists = true,
            kind,
            fileName = attachment.FileName,
            contentType = attachment.ContentType,
            viewUrl = $"/api/recipes/{Uri.EscapeDataString(recipe.Slug)}/source"
        });
    }

    private static async Task<IResult> GetRecipeSourceAsync(
        string id,
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
                a.Category == SourceCategory &&
                a.EntityType == EntityType &&
                a.EntityId == recipe.Slug &&
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

        if (attachment.ContentType.Equals(
                "text/uri-list",
                StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(
                download.Content,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true);

            var url = (await reader.ReadToEndAsync(cancellationToken)).Trim();

            if (Uri.TryCreate(url, UriKind.Absolute, out var sourceUri) &&
                (sourceUri.Scheme == Uri.UriSchemeHttp ||
                 sourceUri.Scheme == Uri.UriSchemeHttps))
            {
                return Results.Redirect(url);
            }

            return Results.UnprocessableEntity(new
            {
                message = "The saved recipe source URL is invalid."
            });
        }

        return Results.File(
            download.Content,
            attachment.ContentType,
            enableRangeProcessing: true);
    }

    private static async Task<IResult> UploadRecipeSourceAsync(
        string id,
        IFormFile file,
        HttpContext httpContext,
        KitchenExcursionContext kitchenDb,
        LaUltimaExcursionDbContext platformDb,
        IAttachmentStorageService storage,
        CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
            return Results.BadRequest(new { message = "Choose a recipe source to upload." });

        if (file.Length > MaxSourceBytes)
        {
            return Results.BadRequest(new
            {
                message = "Recipe source files must be 20 MB or smaller."
            });
        }

        if (!AllowedSourceContentTypes.Contains(file.ContentType))
        {
            return Results.BadRequest(new
            {
                message = "Recipe sources must be a JPG, PNG, WebP, GIF, PDF, text file, or website link."
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
                a.Category == SourceCategory &&
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
            SourceCategory,
            file.FileName,
            file.ContentType,
            content,
            cancellationToken);

        var attachment = new Attachment
        {
            HouseholdId = recipe.HouseholdId,
            UploadedByUserId = currentUser.Id,
            App = AppName,
            Category = SourceCategory,
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
        await platformDb.SaveChangesAsync(cancellationToken);

        return Results.Ok(new
        {
            attachmentId = attachment.Id,
            fileName = attachment.FileName,
            contentType = attachment.ContentType,
            viewUrl = $"/api/recipes/{Uri.EscapeDataString(recipe.Slug)}/source"
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
