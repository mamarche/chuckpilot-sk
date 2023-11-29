# chuckpilot-sk
WPC 2023 Demo project of the session "Build your Copilot with Semantic Kernel"

# Prerequisites
In order to run the sample you need to have the following services:
- Azure OpenAI Service with a gpt 3.5 model deployed
- Azure AI Search
- Azure Cosmos DB
  
Once you have the services up-and-running, fill the values in the local.appsettings.json file. Your file should looks like this:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",

    "AOAIKey": "[YOUR AZURE OPENAI KEY HERE]",
    "AOAIEndpoint": "[YOUR AZURE OPENAI ENDPOINT HERE]",
    "AOAIApiVersion": "2023-07-01-preview",
    "AOAIDeploymentId": "[YOUR GPT MODEL DEPLOYMENT NAME HERE]",
    "SearchEndpoint": "[YOUR AZURE SEARCH ENDPOINT HERE]",
    "SearchKey": "[YOUR AZURE SEARCH KEY HERE]",
    "SearchIndex": "test01",
    "CosmosEndpoint": "[YOUR COSMOSDB ENDPOINT HERE]",
    "CosmosKey": "[YOUR COSMOSDB KEY HERE]"
  }
}
```

# Demo Steps

Branch|Step Description
:---|---:
00-Start|Project scaffolding
01-ChatWithYourData|Build the kernel and test the first Native Function (ChatWithYourDataPlugin)
02-ChuckJokes|Add and test the second Native Function (ChuckJokesPlugin)
03-LanguagePlugin|Add the Semantic Functions (LanguagePlugin) and test the translation function
04-ManualChaining|Manual plugin chaining
05-Planner|Remove the manual chaining and implement planner
06-Memories|Refactor ChatWithYourDataPlugin using the new Kernel Memory
07-ChatHistory|Add the chat history using Cosmos DB