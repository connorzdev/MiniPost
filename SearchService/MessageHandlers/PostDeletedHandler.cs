using Contracts;
using Meilisearch;

namespace SearchService.MessageHandlers;

public class PostDeletedHandler(MeilisearchClient searchClient)
{
    public async Task Handle(PostDeleted message)
    {
        var index = searchClient.Index("posts");
        var task = await index.DeleteOneDocumentAsync(message.PostId.ToString());
        await searchClient.WaitForTaskAsync(task.TaskUid);
    }
}
