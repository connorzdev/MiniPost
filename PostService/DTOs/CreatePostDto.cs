using System.ComponentModel.DataAnnotations;
using Shared;

namespace PostService.DTOs;

public record CreatePostDto(
    [Required] string Title,
    [Required] string Content,
    [Required] PostCategory Category
);
