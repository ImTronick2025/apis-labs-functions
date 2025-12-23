using System.Text.Json.Serialization;

namespace ApisLabs.Functions;

public class Book
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("isbn")]
    public string Isbn { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public AuthorInfo Author { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; } = new();

    [JsonPropertyName("publicationYear")]
    public int PublicationYear { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("pages")]
    public int? Pages { get; set; }

    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("coverImage")]
    public string? CoverImage { get; set; }

    [JsonPropertyName("available")]
    public bool Available { get; set; } = true;

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("reviewCount")]
    public int ReviewCount { get; set; }

    [JsonPropertyName("price")]
    public PriceInfo? Price { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public class AuthorInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class PriceInfo
{
    [JsonPropertyName("amount")]
    public double? Amount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}

public class BookInput
{
    [JsonPropertyName("isbn")]
    public string? Isbn { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("author")]
    public AuthorInfo? Author { get; set; }

    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    [JsonPropertyName("publicationYear")]
    public int? PublicationYear { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("pages")]
    public int? Pages { get; set; }

    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("coverImage")]
    public string? CoverImage { get; set; }

    [JsonPropertyName("available")]
    public bool? Available { get; set; }

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("reviewCount")]
    public int? ReviewCount { get; set; }

    [JsonPropertyName("price")]
    public PriceInfo? Price { get; set; }
}
