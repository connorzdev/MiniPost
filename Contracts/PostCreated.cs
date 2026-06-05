using Shared;

namespace Contracts;

public record PostCreated(
    Guid PostId,
    string Title,
    string Content,
    DateTime Created,
    PostCategory Category
);
