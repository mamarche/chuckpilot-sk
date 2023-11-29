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

            //get the function references
            var joke = kernel.Functions.GetFunction("ChuckJokesPlugin", "RandomJoke");
            var completion = kernel.Functions.GetFunction("ChatWithYourDataPlugin", "GetCompletion");
            var detect = kernel.Functions.GetFunction("LanguagePlugin", "DetectLanguage");
            var translate = kernel.Functions.GetFunction("LanguagePlugin", "Translate");
            var intent = kernel.Functions.GetFunction("LanguagePlugin", "GetIntent");

            //set the context variables
            ContextVariables context = new ContextVariables {
                { "input" , userPrompt.Content },
                { "options" , "GeneralQuestion , Joke" },
                { "conversationId" , userPrompt.ConversationId },
                { "language", docsLanguage }
            };

            //execute the language detection function
            var result = await kernel.RunAsync(context, detect);
            string userLanguage = result.GetValue<string>();
            string userMessage = userPrompt.Content;

            //reset the "input" variable to the user message
            context["input"] = userMessage;

            //if the language of the user question is different from the language of the documents (english),
            //translate into the correct language
            if (userLanguage != docsLanguage)
            {
                //execute the translate function
                result = await kernel.RunAsync(context, translate);
                userMessage = result.GetValue<string>();
            }

            //execute the intent function to understand the user intent
            result = await kernel.RunAsync(context, intent);

            //reset the "input" variable to the user message
            context["input"] = userMessage;

            //if the user wants to get a joke run the joke function,
            //otherwise run the completion function to provide an answer based on the documents
            if (result.GetValue<string>() == "Joke")
            {
                result = await kernel.RunAsync(context, joke);
            }
            else
            {
                result = await kernel.RunAsync(context, completion);
            }

            //if the language of the user question is different from the language of the documents (english),
            //translate into the user language
            if (userLanguage != docsLanguage)
            {
                context["language"] = userLanguage;
                result = await kernel.RunAsync(context, translate);
            }

            //return the final answer
            var resString = result.GetValue<string>();
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(resString);

            return response;
        }
    }
}
