namespace KitchenExcursion.Api.Models;

public class RecipeStatus
{
    public int Id { get; set; }
    public int RecipeRevisionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public RecipeRevision Revision { get; set; } = null!;
}
