namespace KitchenExcursion.Api.Models;

public class RecipeCookRating
{
    public long Id { get; set; }
    public long RecipeCookLogId { get; set; }
    public string Rater { get; set; } = string.Empty;
    public int Stars { get; set; }

    public RecipeCookLog CookLog { get; set; } = null!;
}
