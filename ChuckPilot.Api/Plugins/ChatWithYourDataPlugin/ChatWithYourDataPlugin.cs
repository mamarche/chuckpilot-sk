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
            var messageText = await MemoryManager.AskToMemoryAsync(input);

            return messageText;
        }
    }
}
