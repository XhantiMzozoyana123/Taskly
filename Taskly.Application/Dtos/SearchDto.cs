using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Dtos
{
    public class SearchDto
    {
        [Key]
        public int Id { get; set; }

        public string Keyword { get; set; } = string.Empty; // Keyword to search for

        public string Query { get; set; } = string.Empty; // Query to give more context

        public string CookiePath { get; set; } = string.Empty; // Path to the cookie file for authentication

        public int PageNumber { get; set; } = 1; // For pagination, default to first page

        public bool PrivateMode { get; set; } = false; // Whether to include private content if permissions allow

        public bool HttpMode { get; set; } = false; // Whether to use HTTP mode for requests

        public bool MultiPlatform { get; set; } = false; // Whether to search across multiple platforms
    }
}
