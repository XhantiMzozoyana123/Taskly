using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Taskly.Application.Constants
{
    public static class AppConstants
    {
        public static string ApplicationName = "Taskly AI";

        public static string AppUrl = "https://taskly.com/";

        public static readonly string googleAiEndPoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";

        public static string RemoveAsterisk(string input)
        {
            return input.Replace("*", string.Empty);
        }

        public static string RemoveHashTags(string input)
        {
            return input.Replace("#", string.Empty);
        }

        public static string ConvertStringToHtml(string input, int id, bool tracking)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Escape special characters for HTML
            string encodedText = HttpUtility.HtmlEncode(input);

            // Convert new lines to <br> tags
            encodedText = encodedText.Replace(Environment.NewLine, "<br>");
            encodedText = encodedText.Replace("\n", "<br>");

            // Create a basic HTML structure for an email without a footer
            string htmlTemplate = $@"
                <!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            font-size: 14px;
                            line-height: 1.6;
                            color: #333;
                        }}
                    </style>
                </head>
                <body>
                    <div class=""email-content"">
                        {encodedText}
                    </div>
                </body>
                </html>
            ";

            return htmlTemplate;
        }


        public static string ConvertHtmlToPlainText(string input)
        {
            // Load HTML
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(input);

            // Extract plain text
            string plainText = doc.DocumentNode.InnerText;

            return plainText;
        }
    }
}
