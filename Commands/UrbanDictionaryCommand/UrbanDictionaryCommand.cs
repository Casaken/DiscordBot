using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Discord.Commands;

namespace DiscordBot.Commands.UrbanDictionary
{
    public class UrbanDictionaryCommand : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<UrbanDictionaryCommand> _logger;
        private readonly HttpClient _httpClient;

        public UrbanDictionaryCommand(ILogger<UrbanDictionaryCommand> logger)
        {
            _logger = logger;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiscordBot");
        }

        [Command("cevaplaozan")]
        [Summary("Looks up the phrase using the UrbanDictionary.com API")]
        public async Task ExecuteAsync([Remainder][Summary("A phrase")] string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                await ReplyAsync("Usage: !ud <phrase>");
                return;
            }

            try
            {
                phrase = Uri.EscapeDataString(phrase);

                var response = await _httpClient.GetStringAsync($"http://api.urbandictionary.com/v0/define?term={phrase}");

                if (string.IsNullOrEmpty(response))
                {
                    await ReplyAsync($"Nothing found for {phrase}");
                    return;
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                var result = JsonSerializer.Deserialize<UrbanDictionaryResponse>(response, jsonOptions);

                if (result?.List == null || result.List.Count == 0)
                {
                    await ReplyAsync($"No definition found for {phrase}");
                }
                else
                {
                    await ReplyAsync($"_{phrase}?_");
                    await ReplyAsync(result.List[0].Definition);
                }
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }
    }

    public class UrbanDictionaryResponse
    {
        public List<UrbanDictionaryItem>? List { get; set; }
    }

    public class UrbanDictionaryItem
    {
        public string? Definition { get; set; }
    }
}