using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PostService.Models;

public class Reply
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(5000)]
    public required string Content { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Guid PostId { get; set; }

    [JsonIgnore]
    public Post Post { get; set; } = null!;

    public int LikesCount { get; set; } = 0;
}
