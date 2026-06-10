using Shared;

namespace Contracts;

public record PostUpdated(
    Guid PostId,
    string Title,
    string Content,
    PostCategory Category,
    DateTime UpdatedAt
);
