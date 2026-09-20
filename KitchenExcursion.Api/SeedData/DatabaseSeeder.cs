using KitchenExcursion.Api.Data;

namespace KitchenExcursion.Api.SeedData;

public static class DatabaseSeeder
{
    /// <summary>
    /// Legacy recipes.json importing is intentionally disabled.
    ///
    /// Kitchen Excursion now stores recipe identity separately from immutable
    /// recipe revisions, and recipes are owned by households. Existing SQL
    /// recipes will be converted to revision 1 by the EF migration for the
    /// revision feature. Re-importing the old JSON at application startup would
    /// bypass household ownership and revision history.
    ///
    /// Keep this method so existing startup code can continue calling it safely.
    /// </summary>
    public static Task ImportLegacyRecipesAsync(
        KitchenExcursionContext context,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
