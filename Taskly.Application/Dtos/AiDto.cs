using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Domain.Entities;

namespace Taskly.Application.Dtos
{
    public class AiDto
    {
        public string Prompt { get; set; } = string.Empty;

        public Leads Leads { get; set; } = new Leads();
    }
}
