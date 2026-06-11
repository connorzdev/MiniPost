using JasperFx.Aspire;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder
    .AddDockerComposeEnvironment("production")
    .WithDashboard(dash => dash.WithHostPort(8080));

var keycloak = builder
    .AddKeycloak("keycloak", 6001)
    .WithDataVolume("post-keycloak-data")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithRealmImport("../infra/realms")
    .WithExternalHttpEndpoints();

var meilisearch = builder
    .AddMeilisearch("meilisearch")
    .WithDataVolume("post-meilisearch-data")
    .WithExternalHttpEndpoints();

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
    .WithJasperFxCommands();

var searchService = builder
    .AddProject<SearchService>("search-service")
    .WithReference(meilisearch)
    .WithReference(rabbitMq)
    .WaitFor(meilisearch)
    .WaitFor(rabbitMq)
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

builder.Build().Run();
