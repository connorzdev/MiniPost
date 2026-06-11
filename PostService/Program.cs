using Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PostService.Data;
using Shared.Extensions;
using Shared.Helper;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.Services.AddScoped<PostService.Services.PostService>();
builder.Services.AddKeycloakAuthentication();

var dbConnectionString =
    builder.Configuration.GetConnectionString("postDb")
    ?? throw new Exception("Cannot find post service db connection string");

builder.Services.AddDbContext<PostDbContext>(
    opt =>
    {
        opt.UseNpgsql(dbConnectionString);
    },
    optionsLifetime: ServiceLifetime.Singleton
);

await builder.UseWolverineWithRabbitMqAsync(opt =>
{
    opt.ApplicationAssembly = typeof(Program).Assembly;
    opt.PersistMessagesWithPostgresql(dbConnectionString);
    opt.UseEntityFrameworkCoreTransactions();

    opt.PublishMessage<PostCreated>().ToRabbitExchange("Contracts.PostCreated").UseDurableOutbox();
    opt.PublishMessage<PostUpdated>().ToRabbitExchange("Contracts.PostUpdated").UseDurableOutbox();
    opt.PublishMessage<PostDeleted>().ToRabbitExchange("Contracts.PostDeleted").UseDurableOutbox();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(b =>
    b.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var response = exception switch
        {
            Shared.Exception.BadRequestException e => (Status: 400, e.Message),
            Shared.Exception.UnAuthorizedException e => (Status: 401, e.Message),
            Shared.Exception.ForbiddenException e => (Status: 403, e.Message),
            Shared.Exception.NotFoundException e => (Status: 404, e.Message),

            _ => (Status: 500, Message: "Internal Server Error"),
        };
        context.Response.StatusCode = response.Status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = response.Message });
    })
);

app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

await app.MigrateDbContextAsync<PostDbContext>();

app.Run();
