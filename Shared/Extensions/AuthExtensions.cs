using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Shared.Extensions;

public static class AuthExtensions
{
    public static void AddKeycloakAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakJwtBearer(
                serviceName: "keycloak",
                realm: "post",
                options =>
                {
                    options.Audience = "post";
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuers =
                        [
                            "http://localhost:6001/realms/post",
                            "https://localhost:6001/realms/post",
                        ],
                    };
                }
            );

        services.AddAuthorizationBuilder();
    }
}
