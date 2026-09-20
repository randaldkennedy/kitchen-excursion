namespace KitchenExcursion.Api.Models;

public class RecipeRevision
{
    public int RecipeRevisionId { get; set; }
    public int RecipeId { get; set; }
    public int RevisionNumber { get; set; }

    public int CreatedByUserId { get; set; }
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? ChangeNote { get; set; }

    public required string Title { get; set; }
    public string? Summary { get; set; }
    public string? Badge { get; set; }
    public string? ImageAlt { get; set; }
    public string? PrepTime { get; set; }
    public string? CookTime { get; set; }
    public string? Serves { get; set; }

    public string? Meal { get; set; }
    public string? Protein { get; set; }
    public string? Method { get; set; }
    public string? GeneralNotes { get; set; }

    public Recipe Recipe { get; set; } = null!;

    public ICollection<RecipeCategory> Categories { get; set; } = new List<RecipeCategory>();
    public ICollection<RecipeStatus> Statuses { get; set; } = new List<RecipeStatus>();
    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();
    public ICollection<RecipeShoppingItem> ShoppingItems { get; set; } = new List<RecipeShoppingItem>();
}
