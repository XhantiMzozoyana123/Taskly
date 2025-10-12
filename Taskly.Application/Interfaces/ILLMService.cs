using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Application.Interfaces
{
    public interface ILLMService
    {
        Task<string> GenerateTextAsync(string prompt);

        Task<string> GenerateTextFromImageAsync(string prompt, string base64Image);
    }
}
