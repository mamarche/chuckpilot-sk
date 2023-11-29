using System.Net;
using System.Text.Json;
using ChuckPilot.Api.Plugins;
using ChuckPilot.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;

namespace ChuckPilot.Api
{
    public class Assistant
    {
        private readonly ILogger _logger;

        public Assistant(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Assistant>();
        }

        [Function("ChuckChat")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            //get the UserPrompt object from the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var userPrompt = JsonSerializer.Deserialize<Message>(requestBody);

            //build the kernel using Azure OpenAI service
            var builder = new KernelBuilder();
            builder.WithAzureOpenAIChatCompletionService(
                Environment.GetEnvironmentVariable("AOAIDeploymentId"),
                Environment.GetEnvironmentVariable("AOAIEndpoint"),
                Environment.GetEnvironmentVariable("AOAIKey")
            );
            IKernel kernel = builder.Build();

            //get the path of the "Plugins" folder
            var pluginsDirectory = LocalFiles.GetPath("Plugins");

            //import native functions in the kernel
            kernel.ImportFunctions(new ChatWithYourDataPlugin(), "ChatWithYourDataPlugin");

            //get the function references
            var completion = kernel.Functions.GetFunction("ChatWithYourDataPlugin", "GetCompletion");

            //set the context variables
            ContextVariables context = new ContextVariables {
                { "input" , userPrompt.Content },
                { "conversationId" , userPrompt.ConversationId }
            };

            //execute the joke function
            var result = await kernel.RunAsync(context, completion);
            string completionResult = result.GetValue<string>();

            //return the final answer
            var resString = result.GetValue<string>();
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(completionResult);

            return response;
        }
    }
}
