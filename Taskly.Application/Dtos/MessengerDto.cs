using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Domain.Entities;

namespace Taskly.Application.Dtos
{
    public class MessengerDto
    {
        public string UserId { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public Leads Lead { get; set; } = new Leads();

        public bool AbTestRotation { get; set; } = false;

        public bool PrivateMode { get; set; } = true;
    }
}
