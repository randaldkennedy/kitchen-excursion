namespace KitchenExcursion.Api.Models;

public class RecipeStep
{
    public int Id { get; set; }
    public int RecipeRevisionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public RecipeRevision Revision { get; set; } = null!;
}
