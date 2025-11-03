using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Dtos
{
    public class CookieDto
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;

        [JsonProperty("domain")]
        public string Domain { get; set; } = ".facebook.com";

        [JsonProperty("path")]
        public string Path { get; set; } = "/";

        [JsonProperty("httpOnly")]
        public bool HttpOnly { get; set; } = true;

        [JsonProperty("secure")]
        public bool Secure { get; set; } = true;
    }
}
