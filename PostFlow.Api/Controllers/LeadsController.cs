using Microsoft.AspNetCore.Mvc;
using Taskly.Application.Interfaces;
using Taskly.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PostFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeadsController : ControllerBase
    {
        private readonly IEntityService<Leads> _leadService;

        public LeadsController(IEntityService<Leads> leadService)
        {
            _leadService = leadService;
        }

        [HttpPost]
        public async Task<IActionResult> PostLead([FromBody] Leads lead)
        {
            if (lead == null)
            {
                return BadRequest("Lead data is null.");
            }

            // Basic validation (can be expanded)
            if (string.IsNullOrEmpty(lead.Name) || string.IsNullOrEmpty(lead.ProfileUrl))
            {
                return BadRequest("Lead name and profile URL are required.");
            }

            await _leadService.AddAsync(lead);
            return CreatedAtAction(nameof(GetLeadById), new { id = lead.Id }, lead);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLeadById(int id)
        {
            var lead = await _leadService.GetByIdAsync(id);
            if (lead == null)
            {
                return NotFound();
            }
            return Ok(lead);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLeads()
        {
            var leads = await _leadService.GetAllAsync();
            return Ok(leads);
        }
    }
}
