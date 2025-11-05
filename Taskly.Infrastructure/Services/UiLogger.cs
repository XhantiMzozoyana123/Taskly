using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;

namespace Taskly.Infrastructure.Services
{
    public class UiLogger : IUiLogger
    {
        public BindingList<string> Logs { get; } = new BindingList<string>();

        private void AddLog(string level, string message)
        {
            Logs.Add($"{DateTime.Now:HH:mm:ss} [{level}] {message}");
        }

        public void ClearLogs()
        {
            Logs.Clear();
        }
        public void LogInfo(string message) => AddLog("INFO", message);
        public void LogWarning(string message) => AddLog("WARN", message);
        public void LogError(string message) => AddLog("ERROR", message);
    }
}
