using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostService.DTOs;
using PostService.Extensions;
using PostService.Models;

namespace PostService.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController(Services.PostService service) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost(CreatePostDto dto)
    {
        var userId = User.GetUserId();
        var post = await service.CreatePost(userId, dto);
        return Created($"/posts/{post.Id}", post);
    }

    [HttpGet]
    public async Task<ActionResult<List<Post>>> GetPosts(string? category)
    {
        return await service.GetPosts(category);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Post>> GetPostById(Guid id)
    {
        return await service.GetPostById(id);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdatePost(Guid id, CreatePostDto dto)
    {
        var userId = User.GetUserId();
        await service.UpdatePost(userId, id, dto);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeletePost(Guid id)
    {
        var userId = User.GetUserId();
        await service.DeletePost(userId, id);

        return NoContent();
    }
}
