using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;

namespace Taskly.Application.Interfaces
{
    public interface IUiLogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);

        void ClearLogs();

        BindingList<string> Logs { get; }
    }
}
