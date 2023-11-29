using ChuckPilot.Core;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace ChuckPilot.Api.Plugins
{
    public class ChuckJokesPlugin
    {
        [SKFunction, Description("Get a random joke about Chuck Norris")]
        public async Task<string> RandomJokeAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://api.chucknorris.io/jokes/random");
                var json = await response.Content.ReadAsStringAsync();
                var chuckData = JsonSerializer.Deserialize<ChuckData>(json);
                return chuckData.value;
            }
        }
    }
}
