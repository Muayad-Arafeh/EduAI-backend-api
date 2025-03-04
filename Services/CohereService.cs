using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json.Serialization;

namespace EduAIAPI.Services
{
    public class CohereService : ICohereService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CohereService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Cohere:ApiKey"] ?? throw new ArgumentNullException("Cohere API key is missing.");
        }

        public async Task<string> GetResponseAsync(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    prompt = prompt,
                    max_tokens = 300 // Increased to 300 for longer responses
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.PostAsync("https://api.cohere.ai/v1/generate", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Cohere API Response: " + responseBody); // Log the raw response

                var result = JsonSerializer.Deserialize<CohereResponse>(responseBody);

                // Log the deserialized result
                Console.WriteLine("Deserialized Result: " + JsonSerializer.Serialize(result));

                // Check if the response contains valid data
                if (result == null || result.Generations == null || result.Generations.Length == 0)
                {
                    Console.WriteLine("No valid generations found in the API response.");
                    return "No response from AI.";
                }

                var generation = result.Generations[0];
                if (string.IsNullOrEmpty(generation.Text))
                {
                    Console.WriteLine("The generation text is null or empty.");
                    return "No response from AI.";
                }

                return generation.Text;
            }
            catch (HttpRequestException ex)
            {
                // Handle API request errors
                Console.WriteLine($"Error calling Cohere API: {ex.Message}");
                return $"Error calling Cohere API: {ex.Message}";
            }
            catch (JsonException ex)
            {
                // Handle JSON deserialization errors
                Console.WriteLine($"Error deserializing Cohere API response: {ex.Message}");
                return $"Error deserializing Cohere API response: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Handle other errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                return $"An error occurred: {ex.Message}";
            }
        }

        private class CohereResponse
        {
            [JsonPropertyName("generations")]
            public Generation[]? Generations { get; set; } // Match the API response property name
        }

        private class Generation
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; } // Match the API response property name
        }
    }
}