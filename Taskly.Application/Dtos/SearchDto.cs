using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Dtos
{
    public class SearchDto
    {
        public string UserId { get; set; } = string.Empty; // ID of the user making the request

        public string Keyword { get; set; } = string.Empty; // Keyword to search for

        public string Query { get; set; } = string.Empty; // Query to give more context

        public string Platform { get; set; } = string.Empty; // Social media platform (e.g., Facebook, Instagram)

        public string CookiePath { get; set; } = string.Empty; // Path to the cookie file for authentication

        public int PageNumber { get; set; } = 1; // For pagination, default to first page

        public bool PrivateMode { get; set; } = false; // Whether to include private content if permissions allow
    }
}
