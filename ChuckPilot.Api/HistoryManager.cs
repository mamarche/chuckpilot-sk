using ChuckPilot.Core;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace ChuckPilot.Api
{
    internal static class HistoryManager
    {
        internal static async Task<History> GetHistoryAsync(string conversationId)
        {
            History history;

            if (string.IsNullOrEmpty(conversationId))
            {
                history = new History();
                history.ConversationId = Guid.NewGuid().ToString();
            }
            else
            {
                CosmosClient client = new(accountEndpoint: Environment.GetEnvironmentVariable("CosmosEndpoint"),
                                authKeyOrResourceToken: Environment.GetEnvironmentVariable("CosmosKey")
                            );
                Database database = client.GetDatabase("chuckpilot-db");
                var container = database.GetContainer("histories");
                var query = new QueryDefinition(
                    query: "Select * from histories h where h.conversationId=@id"
                )
                .WithParameter("@id", conversationId);
                using var feed = container.GetItemQueryIterator<HistoryMessage>(
                    queryDefinition: query
                );
                var response = await feed.ReadNextAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    history = new History()
                    {
                        ConversationId = conversationId,
                        Messages = response.Resource.ToList()
                    };
                }
                else
                {
                    history = new History();
                    history.ConversationId = Guid.NewGuid().ToString();
                }
            }

            return history;
        }

        internal static async Task UpdateHistoryAsync(string userMessage, string assistantMessage, string conversationId)
        {
            CosmosClient cosmosClient = new(accountEndpoint: Environment.GetEnvironmentVariable("CosmosEndpoint"),
                authKeyOrResourceToken: Environment.GetEnvironmentVariable("CosmosKey")
            );
            Database database = cosmosClient.GetDatabase("chuckpilot-db");
            var container = database.GetContainer("histories");

            try
            {
                await container.CreateItemAsync(new HistoryMessage()
                {
                    id = Guid.NewGuid().ToString(),
                    conversationId = conversationId,
                    role = "user",
                    content = userMessage
                });
                await container.CreateItemAsync(new HistoryMessage()
                {
                    id = Guid.NewGuid().ToString(),
                    conversationId = conversationId,
                    role = "assistant",
                    content = assistantMessage
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
