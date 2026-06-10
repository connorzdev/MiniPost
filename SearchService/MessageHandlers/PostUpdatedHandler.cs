using Contracts;
using Meilisearch;

namespace SearchService.MessageHandlers;

public class PostUpdatedHandler(MeilisearchClient searchClient)
{
    public async Task Handle(PostUpdated message)
    {
        var updated = new DateTimeOffset(message.UpdatedAt).ToUnixTimeSeconds();

        var index = searchClient.Index("posts");
        var task = await index.UpdateDocumentsAsync(
            [
                new
                {
                    Id = message.PostId,
                    message.Title,
                    message.Content,
                    message.Category,
                    UpdatedAt = updated,
                },
            ]
        );
        await searchClient.WaitForTaskAsync(task.TaskUid);
    }
}
