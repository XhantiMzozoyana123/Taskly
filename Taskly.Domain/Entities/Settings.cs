using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class Settings : BaseEntity
    {
        public string MasterDomainUrl { get; set; } = string.Empty;

        public bool ProcessDataOnline { get; set; } = false;

        public bool DomainRotateWhenExtractingOnline { get; set; } = true;

        public bool CookieRotateWhenExtractingOnline { get; set; } = true;

        public int MessagingDelayInMinutes { get; set; }

        public bool RandomlySelectCookiesForMessaging { get; set; } = true;

        public bool APIKeyRotateWhenUsingGemini { get; set; } = true;

        public bool UseLMStudio { get; set; } = false;
    }
}
