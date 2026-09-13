using System.Text.Json.Serialization;

namespace KitchenExcursion.Api.SeedData;

public class SeedRecipe
{
    [JsonPropertyName("id")]
    public required string Slug { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = [];

    [JsonPropertyName("meal")]
    public string? Meal { get; set; }

    [JsonPropertyName("protein")]
    public string? Protein { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("status")]
    public List<string> Status { get; set; } = [];

    [JsonPropertyName("badge")]
    public string? Badge { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("imageAlt")]
    public string? ImageAlt { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("prep")]
    public string? PrepTime { get; set; }

    [JsonPropertyName("cook")]
    public string? CookTime { get; set; }

    [JsonPropertyName("serves")]
    public string? Serves { get; set; }

    [JsonPropertyName("ingredients")]
    public List<string> Ingredients { get; set; } = [];

    [JsonPropertyName("steps")]
    public List<string> Steps { get; set; } = [];

    [JsonPropertyName("journal")]
    public SeedJournal? Journal { get; set; }

    [JsonPropertyName("shopping")]
    public List<string> Shopping { get; set; } = [];
}

public class SeedJournal
{
    [JsonPropertyName("general")]
    public string? General { get; set; }

    [JsonPropertyName("cookLog")]
    public List<SeedCookLog> CookLog { get; set; } = [];
}

public class SeedCookLog
{
    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}
