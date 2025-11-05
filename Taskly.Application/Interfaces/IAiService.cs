using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Domain.Entities;

namespace Taskly.Application.Interfaces
{
    public interface IAiService
    {
        public Task<List<PostContentDto>> GeneratePostsTextContentAsync(string content);

        public Task<string> GenerateDirectMessageAsync(AiDto aiDto);

        public Task<bool> CheckIfContentIsRelevantAsync(string content, string topic);

        public Task<string> ConvertImageToText(string base64Image);
    }
}
