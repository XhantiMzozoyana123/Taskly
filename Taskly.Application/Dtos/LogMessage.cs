using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Dtos
{
    public class LogMessage
    {
        public DateTime Timestamp { get; }
        public string Level { get; }
        public string Message { get; }

        public LogMessage(string level, string message)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Message = message;
        }

        public override string ToString() => $"{Timestamp:HH:mm:ss} [{Level}] {Message}";
    }
}
