using System.ComponentModel.DataAnnotations;
using Shared;

namespace PostService.Models;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(300)]
    public required string Title { get; set; }

    [MaxLength(5000)]
    public required string Content { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int ViewCount { get; set; } = 0;

    public int LikesCount { get; set; } = 0;

    public PostCategory PostCategory { get; set; }

    public List<Reply> Replies { get; set; } = [];
}
