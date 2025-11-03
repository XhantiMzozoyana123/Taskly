using Microsoft.AspNetCore.Mvc;
using Taskly.Application.Interfaces;
using Taskly.Application.Dtos; // Assuming SearchDto and MessengerDto are here
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PostFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("check-relevance")]
        public async Task<IActionResult> CheckContentRelevance([FromBody] AiRelevanceCheckDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Content) || string.IsNullOrEmpty(dto.Topic))
            {
                return BadRequest("Content and topic are required for relevance check.");
            }

            var isRelevant = await _aiService.CheckIfContentIsRelevantAsync(dto.Content, dto.Topic);
            return Ok(new { isRelevant });
        }

        [HttpPost("generate-dm")]
        public async Task<IActionResult> GenerateDirectMessage([FromBody] SearchDto searchDto) // Using SearchDto for DM generation as per IAiService
        {
            if (searchDto == null || string.IsNullOrEmpty(searchDto.Query))
            {
                return BadRequest("Search query is required for DM generation.");
            }

            var message = await _aiService.GenerateDirectMessageAsync(searchDto);
            return Ok(new { message });
        }
    }

    // DTO for AI relevance check, as IAiService.CheckIfContentIsRelevantAsync takes content and topic
    public class AiRelevanceCheckDto
    {
        public string Content { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
    }
}
