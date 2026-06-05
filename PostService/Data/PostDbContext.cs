using Microsoft.EntityFrameworkCore;
using PostService.Models;

namespace PostService.Data;

public class PostDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Post> Posts { get; set; }

    public DbSet<Reply> Replies { get; set; }
}
