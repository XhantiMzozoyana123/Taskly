using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;

namespace Taskly.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SenderController : ControllerBase
    {
        private readonly ISenderService _senderService;

        public SenderController(ISenderService senderService)
        {
            _senderService = senderService;
        }


        [HttpPost("send")]
        public async Task<IActionResult> StartSequence([FromBody] MessengerDto dto)
        {
            await _senderService.StartMessages(dto);
            return Ok("Sequence started successfully");
        }
    }
}
