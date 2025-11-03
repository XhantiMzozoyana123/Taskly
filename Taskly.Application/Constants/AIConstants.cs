using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Taskly.Application.Dtos;

namespace Taskly.Application.Constants
{
    public static class AIConstants
    {
        public static string PostContentInstructor(string content)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("You are an intelligent content extractor.");
            sb.AppendLine("Analyze the following webpage text content and identify all distinct posts or pieces of user-generated content it contains.");
            sb.AppendLine("For each post, extract the following details:");
            sb.AppendLine("- Author (the name of the person or page that posted it)");
            sb.AppendLine("- Text (main body of the post)");
            sb.AppendLine("- PostUrl (a link to the post if available)");
            sb.AppendLine("- ProfileUrl (a link to the profile or page if available)");
            sb.AppendLine("- PublishedDate (the date and time the post was published if visible, otherwise leave empty)");
            sb.AppendLine();
            sb.AppendLine("Return your answer as a JSON array of objects using the exact following structure:");
            sb.AppendLine(@"[
              {
                ""Author"": ""string"",
                ""Text"": ""string"",
                ""PostUrl"": ""string"",
                ""ProfileUrl"": ""string"",
                ""PublishedDate"": ""-Use the actual real date of the post (datetime datatype)-""
              }
            ]");
            sb.AppendLine();
            sb.AppendLine("Respond only with the JSON array — no explanations, markdown, or extra text.");
            sb.AppendLine();
            sb.AppendLine("Content to analyze:");
            sb.AppendLine(content);

            // Cleanup
            sb.Replace("**", "")
              .Replace("__", "")
              .Replace("*", "")
              .Replace("#", "")
              .Replace("###", "")
              .Replace("##", "")
              .Replace("  ", " ")
              .Replace("\n\n", "\n");

            return sb.ToString();
        }

        public static string IsPostRelevant(string contet, string request)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{contet}: Is this post relevant to this user's request: {request}");
            sb.Append(" Answer with a simple 'True' or 'False'.");
            sb.Append(" Do not provide any additional explanation or context.");
            
            sb.Replace("**", "");
            sb.Replace("__", "");
            sb.Replace("*", "");
            sb.Replace("#", "");
            sb.Replace("###", "");
            sb.Replace("##", "");
            sb.Replace("  ", " ");
            sb.Replace("\n\n", "\n");
            
            return sb.ToString();
        }

        public static string ConvertImageToTextPrompt()
        {
            string result = "Extract all visible text content from this page screenshot.";

            return result;
        }

        public static string BuildDirectMessagePrompt(SearchDto search)
        {
            var sb = new StringBuilder();

            sb.AppendLine("You are an assistant for Taskly, a productivity and automation platform.");
            sb.AppendLine("Your goal is to craft a short, friendly, and personalized direct message to the user based on the following information:");

            sb.AppendLine($"Search Keyword: {search.Keyword}");
            sb.AppendLine($"User Query: {search.Query}");
            sb.AppendLine($"Page Number: {search.PageNumber}");

            sb.AppendLine("\nInstructions for the message:");
            sb.AppendLine("1. The message should directly address the user’s need based on their query and keyword.");
            sb.AppendLine("2. Be concise (under 100 words).");
            sb.AppendLine("3. Maintain a professional yet conversational tone.");
            sb.AppendLine("4. If the URL is provided, reference it naturally as a resource or example.");
            sb.AppendLine("5. Do NOT use markdown, emojis, or special symbols.");
            sb.AppendLine("6. End with a simple call-to-action (e.g., 'Would you like me to explore more on this topic?').");

            sb.AppendLine("\nRespond ONLY with the crafted direct message, nothing else.");

            return sb.ToString();
        }

    }
}
