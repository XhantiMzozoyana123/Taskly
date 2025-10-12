using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;

namespace Taskly.Infrastructure.Services
{
    public class AiService : IAiService
    {
        private readonly ILLMService _llmService;

        public AiService(ILLMService llmService)
        {
            _llmService = llmService;
        }

        public async Task<bool> CheckIfContentIsRelevantAsync(string content, string topic)
        {
            try
            {
                var prompt = AIConstants.IsPostRelevant(content, topic);
                var result = await _llmService.GenerateTextAsync(prompt);

                // Use bool.Parse safely with trimming and casing
                return bool.Parse(result.Trim());
            }
            catch (Exception)
            {
                // Return false if parsing fails
                return false;
            }
        }

        public async Task<string> ConvertImageToText(string base64Image)
        {
            try
            {
                var prompts = AIConstants.ConvertImageToTextPrompt();
                var response = await _llmService.GenerateTextFromImageAsync(prompts, base64Image);

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GenerateDirectMessageAsync(SearchDto searchDto)
        {
            try
            {
                var prompts = AIConstants.BuildDirectMessagePrompt(searchDto);
                var response = await _llmService.GenerateTextAsync(prompts);

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<PostContentDto>> GeneratePostsTextContentAsync(string content)
        {
            try
            {
                var prompts = AIConstants.PostContentInstructor(content);

                var result = await _llmService.GenerateTextAsync(prompts);
                var response = JsonSerializer.Deserialize<List<PostContentDto>>(result);

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
