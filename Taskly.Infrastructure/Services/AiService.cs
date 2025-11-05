using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;

namespace Taskly.Infrastructure.Services
{
    /// <summary>
    /// AiService integrates with a Large Language Model (LLM) service to:
    /// 1. Determine relevance of content to a given topic.
    /// 2. Extract text from images using AI OCR.
    /// 3. Generate personalized direct messages for leads.
    /// 4. Generate structured post content for automation or messaging.
    /// </summary>
    public class AiService : IAiService
    {
        private readonly ILLMService _llmService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AiService"/> class.
        /// </summary>
        /// <param name="llmService">Injected service for interacting with a large language model.</param>
        public AiService(ILLMService llmService)
        {
            _llmService = llmService;
        }

        /// <summary>
        /// Checks whether a given piece of content is relevant to a specified topic.
        /// Uses the LLM to evaluate relevance.
        /// </summary>
        /// <param name="content">The content to evaluate.</param>
        /// <param name="topic">The topic to check relevance against.</param>
        /// <returns>True if the content is relevant; otherwise, false.</returns>
        public async Task<bool> CheckIfContentIsRelevantAsync(string content, string topic)
        {
            try
            {
                // If no topic is specified, treat content as relevant
                if (string.IsNullOrEmpty(topic))
                    return true;

                // Generate prompt instructing LLM to check relevance
                var prompt = AIConstants.IsPostRelevant(content, topic);

                // Call the LLM service to get a response ("true" or "false")
                var result = await _llmService.GenerateTextAsync(prompt);

                // Safely parse the result
                return bool.Parse(result.Trim());
            }
            catch (Exception)
            {
                // On error, default to true (consider content relevant)
                return true;
            }
        }

        /// <summary>
        /// Converts a base64-encoded image into text using AI OCR.
        /// </summary>
        /// <param name="base64Image">The image encoded as a base64 string.</param>
        /// <returns>The extracted text, or null if conversion fails.</returns>
        public async Task<string> ConvertImageToText(string base64Image)
        {
            try
            {
                // Build prompt for image-to-text conversion
                var prompt = AIConstants.ConvertImageToTextPrompt();

                // Use LLM service to extract text from the image
                return await _llmService.GenerateTextFromImageAsync(prompt, base64Image);
            }
            catch (Exception)
            {
                // Return null on failure
                return null;
            }
        }

        /// <summary>
        /// Generates a personalized direct message for a lead based on profile or search info.
        /// </summary>
        /// <param name="aiDto">DTO containing lead/search data.</param>
        /// <returns>The generated message, or null if generation fails.</returns>
        public async Task<string> GenerateDirectMessageAsync(AiDto aiDto)
        {
            try
            {
                // Build prompt to instruct LLM to create a personalized message
                var prompt = AIConstants.BuildDirectMessagePrompt(aiDto);

                // Generate message using LLM
                return await _llmService.GenerateTextAsync(prompt);
            }
            catch (Exception)
            {
                // Fail silently
                return null;
            }
        }

        /// <summary>
        /// Generates structured post content from raw input text.
        /// Useful for automated messaging, captions, or scraping content for DMs.
        /// </summary>
        /// <param name="content">Raw input text to transform into structured content.</param>
        /// <returns>A list of <see cref="PostContentDto"/> objects, or null if generation fails.</returns>
        public async Task<List<PostContentDto>> GeneratePostsTextContentAsync(string content)
        {
            try
            {
                // Build prompt instructing LLM to generate structured content
                var prompt = AIConstants.PostContentInstructor(content);

                // Call LLM and get JSON response
                var result = await _llmService.GenerateTextAsync(prompt);

                // Deserialize JSON into DTO list
                return JsonSerializer.Deserialize<List<PostContentDto>>(result);
            }
            catch (Exception)
            {
                // Return null if AI call or deserialization fails
                return null;
            }
        }
    }
}
