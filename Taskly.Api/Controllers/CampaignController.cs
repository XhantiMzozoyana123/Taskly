using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application.Interfaces;
using Taskly.Domain.Entities;

namespace Taskly.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;

        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        // Pause campaign (pass Campaign object in body)
        [HttpPost("pause")]
        public async Task<IActionResult> PauseCampaign([FromBody] Campaigns campaign)
        {
            if (campaign == null || campaign.Id == 0) return BadRequest("Invalid campaign object");

            await _campaignService.PauseCampaignAsync(campaign);
            return Ok("Campaign paused successfully");
        }

        // Resume campaign
        [HttpPost("resume")]
        public async Task<IActionResult> ResumeCampaign([FromBody] Campaigns campaign)
        {
            if (campaign == null || campaign.Id == 0) return BadRequest("Invalid campaign object");

            await _campaignService.ResumeCampaignAsync(campaign);
            return Ok("Campaign resumed successfully");
        }

        // Run / schedule all sequences in a campaign
        [HttpPost("run")]
        public async Task<IActionResult> RunCampaign([FromBody] Campaigns campaign)
        {
            if (campaign == null || campaign.Id == 0) return BadRequest("Invalid campaign object");

            await _campaignService.StartCampaignAsync(campaign);
            return Ok("Campaign scheduled successfully");
        }
    }
}
