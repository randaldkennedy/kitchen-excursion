namespace KitchenExcursion.Api.Models;

public class Recipe
{
    public int RecipeId { get; set; }

    public required string Slug { get; set; }
    public required string Title { get; set; }

    public string? Summary { get; set; }
    public string? Badge { get; set; }
    public string? Image { get; set; }
    public string? ImageAlt { get; set; }
    public string? PrepTime { get; set; }
    public string? CookTime { get; set; }
    public string? Serves { get; set; }

    public string? Meal { get; set; }
    public string? Protein { get; set; }
    public string? Method { get; set; }
    public string? GeneralNotes { get; set; }

    public ICollection<RecipeCategory> Categories { get; set; } = new List<RecipeCategory>();
    public ICollection<RecipeStatus> Statuses { get; set; } = new List<RecipeStatus>();
    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();
    public ICollection<RecipeShoppingItem> ShoppingItems { get; set; } = new List<RecipeShoppingItem>();
    public ICollection<RecipeCookLog> CookLogs { get; set; } = new List<RecipeCookLog>();
}
