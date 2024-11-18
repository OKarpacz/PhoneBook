using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace PhoneBook.Services
{
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;

        public OpenAiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrEmpty(_openAiApiKey))
            {
                Console.WriteLine("OpenAI API Key is missing.");
                throw new Exception("OpenAI API key not set in environment variables.");
            }

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
        }

        public async Task<string> GetResponseFromLLM(string prompt)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 100,
                temperature = 0.5
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
                var resultText = responseData.GetProperty("choices")[0].GetProperty("message").GetProperty("content")
                    .GetString()?.Trim();
                return resultText;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error from OpenAI API: {error}");
            }
        }
    }
}
