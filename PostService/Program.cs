using Microsoft.AspNetCore.Diagnostics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PostService.Data;
using Shared.Helper;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.Services.AddScoped<PostService.Services.PostService>();
builder
    .Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: "post",
        options =>
        {
            options.Audience = "post";
            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        }
    );

builder.AddNpgsqlDbContext<PostDbContext>("postDb");

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
    opts.PublishAllMessages().ToRabbitExchange("posts");
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
