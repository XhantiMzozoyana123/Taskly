using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;

namespace Taskly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExtractController : ControllerBase
    {
        private readonly IExtractService _extractService;

        public ExtractController(IExtractService extractService)
        {
            _extractService = extractService;
        }

        // POST: api/Extract
        [HttpPost]
        public async Task<IActionResult> ExtractAsync([FromBody] SearchDto searchDto)
        {
            if (searchDto == null)
                return BadRequest("Search criteria is required.");

            await _extractService.ExtractAsync(searchDto);

            return Ok();
        }
    }
}
