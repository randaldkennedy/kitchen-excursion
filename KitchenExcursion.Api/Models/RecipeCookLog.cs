namespace KitchenExcursion.Api.Models;

public class RecipeCookLog
{
    public long Id { get; set; }
    public int RecipeId { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTimeOffset CookedAt { get; set; } = DateTimeOffset.Now;
    public string Author { get; set; } = "Randy";
    public string Note { get; set; } = string.Empty;

    public Recipe Recipe { get; set; } = null!;
    public ICollection<RecipeCookRating> Ratings { get; set; } = new List<RecipeCookRating>();
}
