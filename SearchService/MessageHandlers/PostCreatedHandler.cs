using Contracts;
using Meilisearch;
using SearchService.Models;

namespace SearchService.MessageHandlers;

public class PostCreatedHandler(MeilisearchClient searchClient)
{
    public async Task Handle(PostCreated message)
    {
        var created = new DateTimeOffset(message.Created).ToUnixTimeSeconds();

        var doc = new SearchPost
        {
            Id = message.PostId,
            Title = message.Title,
            Content = message.Content,
            CreatedAt = created,
            Category = message.Category,
        };

        var index = searchClient.Index("posts");
        var task = await index.AddDocumentsAsync([doc]);
        await searchClient.WaitForTaskAsync(task.TaskUid);
    }
}
