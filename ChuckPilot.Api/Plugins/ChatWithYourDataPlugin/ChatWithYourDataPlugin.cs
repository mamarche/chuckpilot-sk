using Azure;
using Azure.AI.OpenAI;
using ChuckPilot.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ChuckPilot.Api.Plugins
{
    public class ChatWithYourDataPlugin
    {

        [SKFunction, Description("Get the completion based on data provided")]
        public async Task<string> GetCompletionAsync([Description("input")] string input, [Description("Id of the conversation")] string conversationId)
        {
            var client = new OpenAIClient(new Uri(Environment.GetEnvironmentVariable("AOAIEndpoint")), new AzureKeyCredential(Environment.GetEnvironmentVariable("AOAIKey")));

            var userMessage = input;

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                AzureExtensionsOptions = new AzureChatExtensionsOptions()
                {
                    Extensions =
                    {
                        new AzureCognitiveSearchChatExtensionConfiguration()
                        {
                            SearchEndpoint = new Uri(Environment.GetEnvironmentVariable("SearchEndpoint")),
                            SearchKey = new AzureKeyCredential(Environment.GetEnvironmentVariable("SearchKey")),
                            IndexName = Environment.GetEnvironmentVariable("SearchIndex"),
                            ShouldRestrictResultScope = true
                        }
                    }
                }
            };
            chatCompletionsOptions.Messages.Add(new Azure.AI.OpenAI.ChatMessage(ChatRole.System, $"You are an assistant and you have to provide information about a person"));

            //add the conversation history
            History history = await HistoryManager.GetHistoryAsync(conversationId);
            foreach (var msg in history.Messages)
            {
                chatCompletionsOptions.Messages.Add(new Azure.AI.OpenAI.ChatMessage(msg.role.ToLower() == "assistant" ? ChatRole.Assistant : ChatRole.User, msg.content));
            }

            chatCompletionsOptions.Messages.Add(new Azure.AI.OpenAI.ChatMessage(ChatRole.User, userMessage));

            var response = await client.GetChatCompletionsAsync(Environment.GetEnvironmentVariable("AOAIDeploymentId"), chatCompletionsOptions);
            var message = response.Value.Choices[0].Message;

            //update the history
            await HistoryManager.UpdateHistoryAsync(userMessage, message.Content, conversationId);

            string messageText = Regex.Replace(message.Content, @"\[\w+\]", "");

            return messageText;
        }
    }
}
