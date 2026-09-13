namespace KitchenExcursion.Api.Models;

public class RecipeIngredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public Recipe Recipe { get; set; } = null!;
}
