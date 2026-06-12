using Aspire.Hosting.Publishing;
using JasperFx.Aspire;
using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder
    .AddDockerComposeEnvironment("production")
    .WithDashboard(dash => dash.WithHostPort(8080));

#pragma warning disable ASPIRECOMPUTE003
#pragma warning disable ASPIREPIPELINES003
var registryEndpoint = builder.AddParameterFromConfiguration(
    "registryEndpoint",
    "REGISTRY_ENDPOINT"
);
var registryRepository = builder.AddParameterFromConfiguration(
    "registryRepository",
    "REGISTRY_REPOSITORY"
);
var registry = builder.AddContainerRegistry(
    "minipost-container-registry",
    registryEndpoint,
    registryRepository
);

var keycloak = builder
    .AddKeycloak("keycloak", 6001)
    .WithDataVolume("post-keycloak-data")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false");

if (!builder.Environment.IsDevelopment())
{
    keycloak
        .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
        .WithEnvironment("KC_HOSTNAME", "https://id-minipost.connordev.cloud")
        .WithExternalHttpEndpoints();
}

var meilisearch = builder.AddMeilisearch("meilisearch").WithDataVolume("post-meilisearch-data");

if (!builder.Environment.IsDevelopment())
{
    meilisearch.WithExternalHttpEndpoints();
}

var postgres = builder.AddPostgres("postgres").WithDataVolume("post-postgres-data").WithPgWeb();
var postDb = postgres.AddDatabase("postDb");

var rabbitMq = builder
    .AddRabbitMQ("messaging")
    .WithDataVolume("post-rabbitmq-data")
    .WithManagementPlugin(port: 15672);

var postService = builder
    .AddProject<PostService>("post-service")
    .WithReference(keycloak)
    .WithReference(postDb)
    .WithReference(rabbitMq)
    .WaitFor(keycloak)
    .WaitFor(postDb)
    .WaitFor(rabbitMq)
    .PublishAsDockerComposeService(
        (_, service) =>
        {
            service.Name = "post-service";
        }
    )
    .WithContainerRegistry(registry)
    .WithContainerBuildOptions(context =>
    {
        context.TargetPlatform = ContainerTargetPlatform.LinuxArm64;
    })
    .WithImagePushOptions(context =>
    {
        context.Options.RemoteImageTag = "latest";
    })
    .WithJasperFxCommands();

var searchService = builder
    .AddProject<SearchService>("search-service")
    .WithReference(meilisearch)
    .WithReference(rabbitMq)
    .WaitFor(meilisearch)
    .WaitFor(rabbitMq)
    .PublishAsDockerComposeService(
        (_, service) =>
        {
            service.Name = "search-service";
        }
    )
    .WithContainerRegistry(registry)
    .WithContainerBuildOptions(context =>
    {
        context.TargetPlatform = ContainerTargetPlatform.LinuxArm64;
    })
    .WithImagePushOptions(context =>
    {
        context.Options.RemoteImageTag = "latest";
    })
    .WithJasperFxCommands();

var gateway = builder
    .AddYarp("gateway")
    .WithHostPort(8001)
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/posts/{**catch-all}", postService);
        yarp.AddRoute("/search/{**catch-all}", searchService);
    })
    .WithExternalHttpEndpoints();

if (!builder.Environment.IsDevelopment())
{
    var tunnelToken = builder.AddParameter("tunnel-token", secret: true);
    var tunnel = builder
        .AddContainer("cloudflared", "cloudflare/cloudflared")
        .WithArgs("tunnel", "--no-autoupdate", "run", "--token", "${TUNNEL_TOKEN}")
        .WithEnvironment("TUNNEL_TOKEN", tunnelToken)
        .WithReference(keycloak)
        .WithReference(gateway)
        .WaitFor(keycloak)
        .WaitFor(gateway);
}
#pragma warning restore ASPIRECOMPUTE003
#pragma warning restore ASPIREPIPELINES003
builder.Build().Run();
