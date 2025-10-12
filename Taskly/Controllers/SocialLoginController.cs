using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application.Interfaces;
using Taskly.Domain.Entities;

namespace Taskly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SocialLoginController : ControllerBase
    {
        private readonly IEntityService<SocialLogins> _service;

        public SocialLoginController(IEntityService<SocialLogins> service) 
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SocialLogins>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SocialLogins>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null)
                return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] SocialLogins entity)
        {
            await _service.AddAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] SocialLogins entity)
        {
            if (id != entity.Id)
                return BadRequest("ID mismatch");

            await _service.UpdateAsync(entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAll([FromBody] SocialLogins entity)
        {
            await _service.DeleteAllAsync(entity);
            return NoContent();
        }
    }
}
