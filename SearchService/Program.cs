using Meilisearch;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SearchService.Models;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.AddMeilisearchClient("meilisearch");

builder
    .Services.AddOpenTelemetry()
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName)
            )
            .AddSource("Wolverine");
    });

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMqUsingNamedConnection("messaging").AutoProvision();
    opts.ListenToRabbitQueue(
        "posts.search",
        cfg =>
        {
            cfg.BindExchange("posts");
        }
    );
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
