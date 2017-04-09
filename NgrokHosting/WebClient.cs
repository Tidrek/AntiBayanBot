using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NgrokHosting
{
    public class WebClient
    {
        private readonly HttpClient _client = new HttpClient();
        private const string BotKey = "AAFYDDIvLccI4iDWIYjlcxM42vBhc8hXvPc";

        public async Task<string> GetNgrokPanelPage()
        {
            var result = await _client.GetAsync("http://localhost:4040/inspect/http");
            return await result.Content.ReadAsStringAsync();
        }

        public async Task UpdateWebHook(string url)
        {
            var result =
                await _client.GetAsync(
                    $"https://api.telegram.org/bot325415087:{BotKey}/setWebhook?url={url}/api/antibayanbot");

            var responce = await result.Content.ReadAsStringAsync();
            var webHookResult = JsonConvert.DeserializeObject<WebHookResult>(responce);

            if(!webHookResult.Ok)
                throw new Exception(webHookResult.Description);
        }
    }
}