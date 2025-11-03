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
    /// <summary>
    /// AiService integrates with an LLM service to generate, evaluate, and process content.
    /// Core responsibilities in Leverage:
    /// 1. Check if a scraped post is relevant to a topic.
    /// 2. Convert images to text using AI.
    /// 3. Generate personalized direct messages for leads.
    /// 4. Generate structured post content for automation or messaging.
    /// </summary>
    public class AiService : IAiService
    {
        private readonly ILLMService _llmService;

        /// <summary>
        /// Constructor for AiService.
        /// </summary>
        /// <param name="llmService">Injected service for interacting with a large language model.</param>
        public AiService(ILLMService llmService)
        {
            _llmService = llmService;
        }

        /// <summary>
        /// Checks if the given content is relevant to a specified topic using the LLM.
        /// </summary>
        /// <param name="content">Text content of the post or lead.</param>
        /// <param name="topic">Topic to check relevance against.</param>
        /// <returns>True if content is relevant; otherwise, false.</returns>
        public async Task<bool> CheckIfContentIsRelevantAsync(string content, string topic)
        {
            try
            {
                // Generate a prompt to evaluate relevance
                var prompt = AIConstants.IsPostRelevant(content, topic);

                // Call the LLM service to get a response (expected "true" or "false")
                var result = await _llmService.GenerateTextAsync(prompt);

                // Parse the result safely
                return bool.Parse(result.Trim());
            }
            catch (Exception ex)
            {
                // Log the exception in future versions if needed
                // Return false on failure to ensure non-relevant default
                return false;
            }
        }

        /// <summary>
        /// Converts a base64-encoded image into text using AI OCR.
        /// </summary>
        /// <param name="base64Image">Base64 string representation of the image.</param>
        /// <returns>Extracted text, or null if conversion fails.</returns>
        public async Task<string> ConvertImageToText(string base64Image)
        {
            try
            {
                // Get prompt for image-to-text conversion
                var prompts = AIConstants.ConvertImageToTextPrompt();

                // Call the LLM service to extract text from the image
                var response = await _llmService.GenerateTextFromImageAsync(prompts, base64Image);

                return response;
            }
            catch (Exception)
            {
                // Return null if extraction fails
                return null;
            }
        }

        /// <summary>
        /// Generates a personalized direct message for a lead based on their profile/content.
        /// </summary>
        /// <param name="searchDto">DTO containing lead/search information.</param>
        /// <returns>Generated message text, or null on failure.</returns>
        public async Task<string> GenerateDirectMessageAsync(SearchDto searchDto)
        {
            try
            {
                // Build prompt for DM generation
                var prompts = AIConstants.BuildDirectMessagePrompt(searchDto);

                // Call LLM to generate text
                var response = await _llmService.GenerateTextAsync(prompts);

                return response;
            }
            catch (Exception)
            {
                // Fail silently; return null
                return null;
            }
        }

        /// <summary>
        /// Generates structured post content (PostContentDto) from raw input text.
        /// Useful for creating messages, captions, or scraping content for DMs.
        /// </summary>
        /// <param name="content">Raw content to generate structured text from.</param>
        /// <returns>List of PostContentDto objects or null on failure.</returns>
        public async Task<List<PostContentDto>> GeneratePostsTextContentAsync(string content)
        {
            try
            {
                // Build the prompt to instruct AI to create structured post content
                var prompts = AIConstants.PostContentInstructor(content);

                // Call the LLM service
                var result = await _llmService.GenerateTextAsync(prompts);

                // Deserialize JSON response into DTO list
                var response = JsonSerializer.Deserialize<List<PostContentDto>>(result);

                return response;
            }
            catch (Exception)
            {
                // Return null if deserialization or AI call fails
                return null;
            }
        }
    }
}
