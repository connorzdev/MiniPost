using System.Text.Json.Serialization;
using Shared;

namespace SearchService.Models;

public class SearchPost
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("category")]
    public PostCategory Category { get; set; }

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public long UpdatedAt { get; set; }
}
