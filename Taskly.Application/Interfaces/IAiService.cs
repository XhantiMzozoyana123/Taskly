using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;

namespace Taskly.Application.Interfaces
{
    public interface IAiService
    {
        public Task<List<PostContentDto>> GeneratePostsTextContentAsync(string content);

        public Task<string> GenerateDirectMessageAsync(SearchDto searchDto);

        public Task<bool> CheckIfContentIsRelevantAsync(string content, string topic);

        public Task<string> ConvertImageToText(string base64Image);
    }
}
