namespace KitchenExcursion.Api.Models;

public class Recipe
{
    public int RecipeId { get; set; }

    public int HouseholdId { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

    public required string Slug { get; set; }

    // Hero image belongs to the recipe identity. Attachment history handles image replacements.
    public string? Image { get; set; }

    // Nullable so a Recipe row can be inserted before its first revision is created.
    public int? CurrentRevisionId { get; set; }
    public RecipeRevision? CurrentRevision { get; set; }

    public ICollection<RecipeRevision> Revisions { get; set; } = new List<RecipeRevision>();
    public ICollection<RecipeCookLog> CookLogs { get; set; } = new List<RecipeCookLog>();
}
