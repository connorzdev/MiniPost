using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostService.Data;
using PostService.DTOs;
using PostService.Models;

namespace PostService.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController(PostDbContext db) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost(CreatePostDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue("name");

        if (userId is null || name is null)
        {
            return BadRequest("Cannot get user details");
        }

        var post = new Post
        {
            Title = dto.Title,
            Content = dto.Content,
            UserId = new Guid(userId),
            PostCategory = dto.Category,
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        // TODO: Add message queue to update elastic search

        return Created($"/posts/{post.Id}", post);
    }
}
