using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Dtos
{
    public class PostContentDto
    {
        public string Author { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string PostUrl { get; set; } = string.Empty;

        public string ProfileUrl { get; set; } = string.Empty;

        public DateTime PublishedDate { get; set; }
    }
}
