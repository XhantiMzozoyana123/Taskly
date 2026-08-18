using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace Taskly.Infrastructure.Services
{
    public class LLMService : ILLMService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public LLMService(
            HttpClient httpClient,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            // Build the request payload
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);

            // Get API key from configuration
            var apiKey = await GetApiKeyAsync();
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Gemini API key is missing in configuration.");

            // Send POST request
            var response = await _httpClient.PostAsync(
                AppConstants.googleAiEndPoint + apiKey,
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: {response.StatusCode}";
            }

            // Parse response JSON
            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "No response";
        }

        public async Task<string> GenerateTextFromImageAsync(string prompt, string base64Image)
        {
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            // 1. The image part
                            new
                            {
                                image = new
                                {
                                    type = "base64",
                                    data = base64Image
                                }
                            },
                            // 2. Instructions part
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var apiKey = await GetApiKeyAsync();
            var response = await _httpClient.PostAsync(
                AppConstants.googleAiEndPoint + apiKey,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode}";

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            // Parse the AI response text
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "No response";
        }


        private async Task<string> GetApiKeyAsync()
        {
            try
            {
                string apiKey = string.Empty;

                var settings = await _context.Settings.FirstOrDefaultAsync() ?? new Taskly.Domain.Entities.Settings();
                bool rotateKeys = settings.APIKeyRotateWhenUsingGemini;
                var googleAi = _context.GoogleAIs.ToList();

                if (rotateKeys)
                {

                    Random r = new Random();
                    int i = r.Next(0, googleAi.Count);

                    apiKey = googleAi[i].ApiKey;
                }
                else
                {
                    apiKey = googleAi.FirstOrDefault().ApiKey;
                }

                return apiKey;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
