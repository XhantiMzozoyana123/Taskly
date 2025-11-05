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
        public string Text { get; set; } = string.Empty;

        public List<string> TextList { get; set; } = new List<string>();

        public Leads Lead { get; set; } = new Leads();

        public bool MessegeRotation { get; set; } = false;

        public bool AccountRotation { get; set; } = false; 

        public bool PrivateMode { get; set; } = false;

        public int MessageDelay { get; set; }
    }
}
