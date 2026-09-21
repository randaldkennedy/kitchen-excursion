using OpenAI;
using OpenAI.Chat;
using PDFtoImage;
using SkiaSharp;
using System.ClientModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KitchenExcursion.Api.Endpoints;

public static class RecipeImportEndpoints
{
    private const long MaxFileBytes = 20 * 1024 * 1024;
    private const int MaxWebsiteChars = 120_000;
    private const int MaxPasteChars = 60_000;

    private static readonly HashSet<string> AllowedFileTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
            "application/pdf"
        };

    public static IEndpointRouteBuilder MapRecipeImportEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/recipe-import/file", ImportFileAsync)
            .DisableAntiforgery()
            .RequireAuthorization();

        app.MapPost("/api/recipe-import/url", ImportUrlAsync)
            .RequireAuthorization();

        app.MapPost("/api/recipe-import/text", ImportTextAsync)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> ImportFileAsync(
        IFormFile file,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
            return Results.BadRequest(new { message = "Choose a recipe image or PDF." });

        if (file.Length > MaxFileBytes)
            return Results.BadRequest(new { message = "Recipe files must be 20 MB or smaller." });

        if (!AllowedFileTypes.Contains(file.ContentType))
        {
            return Results.BadRequest(new
            {
                message = "Use a JPEG, PNG, WebP, GIF, or PDF recipe file."
            });
        }

        var chatClient = CreateChatClient(configuration);

        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var parts = new List<ChatMessageContentPart>
        {
            ChatMessageContentPart.CreateTextPart(BuildPrompt(
                "The source is a photograph or scanned recipe. Read handwriting and printed text carefully."))
        };

        if (string.Equals(
                file.ContentType,
                "application/pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            var pageCount = 0;

            foreach (var bitmap in Conversion.ToImages(
                         buffer,
                         leaveOpen: true,
                         options: new RenderOptions(Dpi: 200)))
            {
                using (bitmap)
                using (var png = new MemoryStream())
                {
                    bitmap.Encode(png, SKEncodedImageFormat.Png, 100);

                    parts.Add(
                        ChatMessageContentPart.CreateImagePart(
                            BinaryData.FromBytes(png.ToArray()),
                            "image/png",
                            ChatImageDetailLevel.High));
                }

                pageCount++;
                if (pageCount >= 6)
                    break;
            }

            if (pageCount == 0)
            {
                return Results.UnprocessableEntity(new
                {
                    message = "Kitchen could not render that PDF."
                });
            }
        }
        else
        {
            parts.Add(
                ChatMessageContentPart.CreateImagePart(
                    BinaryData.FromBytes(buffer.ToArray()),
                    file.ContentType,
                    ChatImageDetailLevel.High));
        }

        var draft = await AnalyzeAsync(
            chatClient,
            parts,
            cancellationToken);

        return draft is null
            ? Results.UnprocessableEntity(new { message = "Kitchen could not read that recipe." })
            : Results.Ok(draft);
    }

    private static async Task<IResult> ImportTextAsync(
        RecipeTextImportRequest request,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var text = request.Text?.Trim();

        if (string.IsNullOrWhiteSpace(text))
            return Results.BadRequest(new { message = "Paste some recipe text first." });

        if (text.Length > MaxPasteChars)
            return Results.BadRequest(new { message = "Pasted recipe text is too long." });

        var chatClient = CreateChatClient(configuration);

        var parts = new List<ChatMessageContentPart>
        {
            ChatMessageContentPart.CreateTextPart(BuildPrompt(
                "The source is pasted recipe text.")),
            ChatMessageContentPart.CreateTextPart(text)
        };

        var draft = await AnalyzeAsync(
            chatClient,
            parts,
            cancellationToken);

        return draft is null
            ? Results.UnprocessableEntity(new { message = "Kitchen could not interpret that recipe text." })
            : Results.Ok(draft);
    }

    private static async Task<IResult> ImportUrlAsync(
        RecipeUrlImportRequest request,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.Url?.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Results.BadRequest(new { message = "Enter a valid http or https recipe URL." });
        }

        if (!IsSafePublicUri(uri))
        {
            return Results.BadRequest(new
            {
                message = "That URL points to a local or private network address."
            });
        }

        var httpClient = httpClientFactory.CreateClient("RecipeImport");

        using var response = await httpClient.GetAsync(
            uri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Results.BadRequest(new
            {
                message = $"Kitchen could not load that page (HTTP {(int)response.StatusCode})."
            });
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is not null &&
            !mediaType.Contains("html", StringComparison.OrdinalIgnoreCase) &&
            !mediaType.Contains("text", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new
            {
                message = "That URL did not return a web page."
            });
        }

        var html = await ReadLimitedTextAsync(
            response.Content,
            MaxWebsiteChars,
            cancellationToken);

        var recipeSource = ExtractRecipeJsonLd(html);

        string sourceDescription;
        string content;

        if (!string.IsNullOrWhiteSpace(recipeSource))
        {
            sourceDescription =
                "The source is structured JSON-LD recipe data extracted from a recipe web page.";
            content = recipeSource;
        }
        else
        {
            sourceDescription =
                "The source is readable text extracted from a recipe web page. Ignore menus, ads, navigation, comments, and unrelated page text.";
            content = StripHtml(html);
        }

        var chatClient = CreateChatClient(configuration);

        var parts = new List<ChatMessageContentPart>
        {
            ChatMessageContentPart.CreateTextPart(BuildPrompt(sourceDescription)),
            ChatMessageContentPart.CreateTextPart(
                $"Source URL: {uri}\n\n{content}")
        };

        var draft = await AnalyzeAsync(
            chatClient,
            parts,
            cancellationToken);

        return draft is null
            ? Results.UnprocessableEntity(new { message = "Kitchen could not find a usable recipe on that page." })
            : Results.Ok(draft);
    }

    private static ChatClient CreateChatClient(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        var key = configuration["AzureOpenAI:Key"];
        var deployment = configuration["AzureOpenAI:Deployment"];

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(key) ||
            string.IsNullOrWhiteSpace(deployment))
        {
            throw new InvalidOperationException(
                "Recipe importing requires AzureOpenAI:Endpoint, AzureOpenAI:Key, and AzureOpenAI:Deployment.");
        }

        endpoint = endpoint.TrimEnd('/') + "/";

        return new ChatClient(
            model: deployment,
            credential: new ApiKeyCredential(key),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint)
            });
    }

    private static async Task<RecipeImportDraft?> AnalyzeAsync(
        ChatClient chatClient,
        IReadOnlyList<ChatMessageContentPart> userParts,
        CancellationToken cancellationToken)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(
                "You extract recipes faithfully into editable Kitchen Excursion drafts. Never invent missing recipe facts."),
            new UserChatMessage(userParts.ToArray())
        ];

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "kitchen_recipe_import",
                jsonSchema: BinaryData.FromBytes("""
                    {
                      "type": "object",
                      "properties": {
                        "title": { "type": "string" },
                        "summary": {
                          "anyOf": [
                            { "type": "string" },
                            { "type": "null" }
                          ]
                        },
                        "meal": {
                          "anyOf": [
                            { "type": "string", "enum": ["breakfast", "lunch", "dinner", "side", "dessert"] },
                            { "type": "null" }
                          ]
                        },
                        "protein": {
                          "anyOf": [
                            { "type": "string", "enum": ["pork", "beef", "chicken", "turkey", "seafood", "meatless", "ham-turkey"] },
                            { "type": "null" }
                          ]
                        },
                        "method": {
                          "anyOf": [
                            { "type": "string", "enum": ["oven", "stovetop", "slow-cooker", "grill", "air-fryer", "microwave"] },
                            { "type": "null" }
                          ]
                        },
                        "categories": {
                          "type": "array",
                          "items": { "type": "string" }
                        },
                        "prep": {
                          "anyOf": [
                            { "type": "string" },
                            { "type": "null" }
                          ]
                        },
                        "cook": {
                          "anyOf": [
                            { "type": "string" },
                            { "type": "null" }
                          ]
                        },
                        "serves": {
                          "anyOf": [
                            { "type": "string" },
                            { "type": "null" }
                          ]
                        },
                        "ingredients": {
                          "type": "array",
                          "items": { "type": "string" }
                        },
                        "shopping": {
                          "type": "array",
                          "items": { "type": "string" }
                        },
                        "steps": {
                          "type": "array",
                          "items": { "type": "string" }
                        },
                        "generalNotes": {
                          "anyOf": [
                            { "type": "string" },
                            { "type": "null" }
                          ]
                        },
                        "warnings": {
                          "type": "array",
                          "items": { "type": "string" }
                        }
                      },
                      "required": [
                        "title",
                        "summary",
                        "meal",
                        "protein",
                        "method",
                        "categories",
                        "prep",
                        "cook",
                        "serves",
                        "ingredients",
                        "shopping",
                        "steps",
                        "generalNotes",
                        "warnings"
                      ],
                      "additionalProperties": false
                    }
                    """u8.ToArray()),
                jsonSchemaIsStrict: true)
        };

        ChatCompletion completion = await chatClient.CompleteChatAsync(
            messages,
            options,
            cancellationToken);

        var draft = JsonSerializer.Deserialize<RecipeImportDraft>(
            completion.Content[0].Text,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (draft is null || string.IsNullOrWhiteSpace(draft.Title))
            return null;

        draft.Title = draft.Title.Trim();
        draft.Summary = Clean(draft.Summary);
        draft.Meal = Clean(draft.Meal);
        draft.Protein = Clean(draft.Protein);
        draft.Method = Clean(draft.Method);
        draft.Prep = Clean(draft.Prep);
        draft.Cook = Clean(draft.Cook);
        draft.Serves = Clean(draft.Serves);
        draft.GeneralNotes = Clean(draft.GeneralNotes);
        draft.Categories = CleanList(draft.Categories);
        draft.Ingredients = CleanList(draft.Ingredients);
        draft.Shopping = CleanList(draft.Shopping);
        draft.Steps = CleanList(draft.Steps);
        draft.Warnings = CleanList(draft.Warnings);

        return draft;
    }

    private static string BuildPrompt(string sourceDescription) =>
        $"""
        Import this recipe into an editable Kitchen Excursion draft.

        {sourceDescription}

        Rules:
        - Extract only information supported by the source.
        - Do not invent prep time, cook time, servings, ingredient quantities, temperatures, or missing directions.
        - Preserve unusual wording or family-specific directions when they appear intentional.
        - Correct obvious spacing/punctuation issues, but do not rewrite the recipe into a different recipe.
        - For handwriting, make the best faithful transcription you can.
        - If a word, quantity, ingredient, or direction is uncertain, keep the most likely transcription when useful AND add a concise warning.
        - ingredients must contain one ingredient per array item and preserve recipe quantities/prep wording from the source.
        - shopping must contain clean grocery purchase identities derived from ingredients, one purchasable need per array item.
        - Shopping quantity rules are mandatory, not optional.
        - ALWAYS preserve a source quantity/count when it determines how much of a primary purchasable item the shopper needs.
        - In particular, ALWAYS preserve quantities for meat, poultry, seafood, produce counts, cans, jars, bags, boxes, packages, and sticks.
        - Examples that MUST preserve quantity:
          - "2 pounds lean ground beef" -> "2 lb lean ground beef"
          - "3 lb chicken breasts" -> "3 lb chicken breasts"
          - "2 cans diced tomatoes" -> "2 cans diced tomatoes"
          - "1 stick butter" -> "1 stick butter"
          - "3 avocados" -> "3 avocados"
        - Omit small recipe measurements only when they describe the amount used from a normal pantry/store container and do not help decide how much to buy.
        - Examples that should omit the recipe measurement:
          - "1 tsp garlic salt" -> "Garlic salt"
          - "1/4 cup ketchup" -> "Ketchup"
          - "1 Tbsp Worcestershire sauce" -> "Worcestershire sauce"
          - "2 eggs" -> "Eggs"
        - Never remove a weight/count from meat, poultry, or seafood when that weight/count is present in the source ingredient.
        - Split combined ingredients into separate shopping items when they are distinct products. Example: "Salt and black pepper to taste" becomes "Salt" and "Black pepper".
        - Preserve meaningful product type or alternatives needed for shopping. Example: "avocado or prepared guacamole" may remain "Avocado or prepared guacamole".
        - Do not add pantry assumptions or omit an ingredient merely because it is commonly kept on hand.
        - steps must contain one actual instruction per array item, without leading step numbers.
        - If the source mixes ingredients and directions, separate them carefully.
        - summary may be a short factual description derived from the recipe, but do not add claims not supported by the source.
        - meal, protein, and method should use only the allowed values; use null when unclear.
        - categories should be short useful labels such as Pizza, Sandwich, Soup, Casserole, Breakfast, Dinner, or Side. Do not over-tag.
        - prep, cook, and serves should preserve the source wording when present; otherwise null.
        - generalNotes is for useful source notes that are not ingredients or steps.
        - warnings should identify ambiguity or missing information the cook should review.
        """;

    private static async Task<string> ReadLimitedTextAsync(
        HttpContent content,
        int maxChars,
        CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var buffer = new char[8192];
        var builder = new StringBuilder();

        while (builder.Length < maxChars)
        {
            var remaining = Math.Min(buffer.Length, maxChars - builder.Length);
            var read = await reader.ReadAsync(
                buffer.AsMemory(0, remaining),
                cancellationToken);

            if (read == 0)
                break;

            builder.Append(buffer, 0, read);
        }

        return builder.ToString();
    }

    private static string? ExtractRecipeJsonLd(string html)
    {
        var matches = Regex.Matches(
            html,
            """<script[^>]*type\s*=\s*["']application/ld\+json["'][^>]*>(.*?)</script>""",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var candidate = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();

            if (candidate.Contains(
                    "\"Recipe\"",
                    StringComparison.OrdinalIgnoreCase))
            {
                return candidate.Length <= MaxWebsiteChars
                    ? candidate
                    : candidate[..MaxWebsiteChars];
            }
        }

        return null;
    }

    private static string StripHtml(string html)
    {
        var withoutScripts = Regex.Replace(
            html,
            "<script\\b[^<]*(?:(?!</script>)<[^<]*)*</script>",
            " ",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        var withoutStyles = Regex.Replace(
            withoutScripts,
            "<style\\b[^<]*(?:(?!</style>)<[^<]*)*</style>",
            " ",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        var text = Regex.Replace(
            withoutStyles,
            "<[^>]+>",
            " ",
            RegexOptions.Singleline);

        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, "\\s+", " ").Trim();

        return text.Length <= MaxWebsiteChars
            ? text
            : text[..MaxWebsiteChars];
    }

    private static bool IsSafePublicUri(Uri uri)
    {
        if (uri.IsLoopback)
            return false;

        if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            foreach (var address in Dns.GetHostAddresses(uri.DnsSafeHost))
            {
                if (IsPrivateAddress(address))
                    return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static bool IsPrivateAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return address.IsIPv6LinkLocal ||
                   address.IsIPv6SiteLocal ||
                   address.Equals(IPAddress.IPv6Loopback);
        }

        var bytes = address.GetAddressBytes();

        return bytes[0] == 10 ||
               bytes[0] == 127 ||
               (bytes[0] == 169 && bytes[1] == 254) ||
               (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
               (bytes[0] == 192 && bytes[1] == 168);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[] CleanList(IEnumerable<string>? values) =>
        values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray()
        ?? Array.Empty<string>();

    public sealed record RecipeTextImportRequest(string? Text);
    public sealed record RecipeUrlImportRequest(string? Url);

    public sealed class RecipeImportDraft
    {
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Meal { get; set; }
        public string? Protein { get; set; }
        public string? Method { get; set; }
        public string[] Categories { get; set; } = Array.Empty<string>();
        public string? Prep { get; set; }
        public string? Cook { get; set; }
        public string? Serves { get; set; }
        public string[] Ingredients { get; set; } = Array.Empty<string>();
        public string[] Shopping { get; set; } = Array.Empty<string>();
        public string[] Steps { get; set; } = Array.Empty<string>();
        public string? GeneralNotes { get; set; }
        public string[] Warnings { get; set; } = Array.Empty<string>();
    }
}
