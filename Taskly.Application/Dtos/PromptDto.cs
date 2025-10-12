using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Dtos
{
    public class PromptDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}
