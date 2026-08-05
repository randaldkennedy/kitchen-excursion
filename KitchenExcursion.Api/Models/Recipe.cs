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
}