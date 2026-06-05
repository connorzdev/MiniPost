var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 6001).WithDataVolume("post-keycloak-data");

builder.Build().Run();
