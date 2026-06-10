using Meilisearch;
using SearchService.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.AddMeilisearchClient("meilisearch");
await builder.UseWolverineWithRabbitMqAsync(opt =>
{
    opt.ApplicationAssembly = typeof(Program).Assembly;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet(
    "/search",
    async (string query, MeilisearchClient meilisearch) =>
    {
        try
        {
            var index = meilisearch.Index("posts");
            var results = await index.SearchAsync<SearchPost>(query);
            return Results.Ok(results.Hits);
        }
        catch (Exception e)
        {
            return Results.Problem("Meilisearch search failed", e.Message);
        }
    }
);

app.MapDefaultEndpoints();

app.Run();
