using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class Settings 
    {
        [Key]
        public int Id { get; set; }

        public int MessagingDelayInMinutes { get; set; }

        public bool RandomlySelectCookiesForMessaging { get; set; } = true;

        public bool APIKeyRotateWhenUsingGemini { get; set; } = true;

        public bool UseLMStudio { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
