using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 6001).WithDataVolume("post-keycloak-data");

var postgres = builder.AddPostgres("postgres").WithDataVolume("post-postgres-data").WithPgWeb();

var postDb = postgres.AddDatabase("postDb");

var postService = builder
    .AddProject<PostService>("post-service")
    .WithReference(keycloak)
    .WithReference(postDb)
    .WaitFor(keycloak)
    .WaitFor(postDb);

builder.Build().Run();
