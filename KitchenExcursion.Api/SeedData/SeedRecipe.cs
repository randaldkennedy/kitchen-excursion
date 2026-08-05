using System.Text.Json.Serialization;

namespace KitchenExcursion.Api.SeedData;

public class SeedRecipe
{
    [JsonPropertyName("id")]
    public required string Slug { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("badge")]
    public string? Badge { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("imageAlt")]
    public string? ImageAlt { get; set; }

    [JsonPropertyName("prep")]
    public string? PrepTime { get; set; }

    [JsonPropertyName("cook")]
    public string? CookTime { get; set; }

    [JsonPropertyName("serves")]
    public string? Serves { get; set; }
}