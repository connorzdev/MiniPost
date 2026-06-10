using System.Security.Claims;
using Shared.Exception;

namespace PostService.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId == null
            ? throw new UnAuthorizedException("User not found")
            : Guid.Parse(userId);
    }
}
