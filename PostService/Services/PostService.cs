using Contracts;
using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.DTOs;
using PostService.Models;
using Shared.Exception;
using Wolverine;

namespace PostService.Services;

public class PostService(PostDbContext db, IMessageBus bus)
{
    public async Task<Post> CreatePost(Guid userId, CreatePostDto dto)
    {
        var post = new Post
        {
            Title = dto.Title,
            Content = dto.Content,
            UserId = userId,
            PostCategory = dto.Category,
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        await bus.PublishAsync(
            new PostCreated(post.Id, post.Title, post.Content, post.CreatedAt, post.PostCategory)
        );

        return post;
    }

    public async Task<List<Post>> GetPosts(string? category)
    {
        var query = db.Posts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.PostCategory.ToString() == category);
        }

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<Post> GetPostById(Guid id)
    {
        var post = await db.Posts.Include(x => x.Replies).FirstOrDefaultAsync(x => x.Id == id);

        if (post is null)
            throw new NotFoundException("Post not found");

        await db
            .Posts.Where(x => x.Id == id)
            .ExecuteUpdateAsync(setter =>
                setter.SetProperty(x => x.ViewCount, x => x.ViewCount + 1)
            );

        post.ViewCount++;
        return post;
    }

    public async Task UpdatePost(Guid userId, Guid id, CreatePostDto dto)
    {
        var post = await db.Posts.FindAsync(id);
        if (post is null)
            throw new NotFoundException("Post not found");

        if (userId != post.UserId)
            throw new ForbiddenException("");

        post.Title = dto.Title;
        post.Content = dto.Content;
        post.PostCategory = dto.Category;
        post.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        await bus.PublishAsync(
            new PostUpdated(
                post.Id,
                post.Title,
                post.Content,
                post.PostCategory,
                (DateTime)post.UpdatedAt
            )
        );
    }

    public async Task DeletePost(Guid userId, Guid id)
    {
        var post = await db.Posts.FindAsync(id);
        if (post is null)
            throw new NotFoundException($"Post with id {id} not found");

        if (userId != post.UserId)
            throw new ForbiddenException("");

        db.Posts.Remove(post);
        await db.SaveChangesAsync();

        await bus.PublishAsync(new PostDeleted(post.Id));
    }
}
