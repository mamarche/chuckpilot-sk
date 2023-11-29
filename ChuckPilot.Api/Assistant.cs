using System.Net;
using System.Text.Json;
using ChuckPilot.Api.Plugins;
using ChuckPilot.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planners;

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
            const string docsLanguage = "english";

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

            //initialize kernel memory
            MemoryManager.InitializeMemory();

            //get the path of the "Plugins" folder
            var pluginsDirectory = LocalFiles.GetPath("Plugins");

            //import native functions in the kernel
            kernel.ImportFunctions(new ChatWithYourDataPlugin(), "ChatWithYourDataPlugin");
            kernel.ImportFunctions(new ChuckJokesPlugin(), "ChuckJokesPlugin");
            //import semantic functions in the kernel
            kernel.ImportSemanticFunctionsFromDirectory(pluginsDirectory, "LanguagePlugin");

            var planner = new StepwisePlanner(kernel);

            //set the context variables
            ContextVariables context = new ContextVariables {
                { "input" , userPrompt.Content },
                { "options" , "GeneralQuestion , Joke" },
                { "conversationId" , userPrompt.ConversationId },
                { "language", docsLanguage }
            };

            var plan = planner.CreatePlan($"Answer to the user question based on the user intent. " +
                $"Always answer in the same language of the user using ONLY the data provided. " +
                $"The data provided is in {docsLanguage}. " +
                $"The user question is: {userPrompt.Content}");
            var planResult = await kernel.RunAsync(context, plan);

            //return the final answer
            var resString = planResult.GetValue<string>();
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(resString);

            return response;
        }
    }
}
