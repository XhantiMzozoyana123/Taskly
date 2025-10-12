using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application.Interfaces;
using Taskly.Domain.Entities;

namespace Taskly.Controllers
{
    /// <summary>
    /// Controller for managing messages.
    /// Requires 'PremiumOnly' subscription policy for all actions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "PremiumOnly")]
    public class MessageController : ControllerBase
    {
        private readonly IEntityService<Messages> _service;

        public MessageController(IEntityService<Messages> service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all messages.
        /// </summary>
        /// <returns>A list of all messages.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Messages>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a message by its ID.
        /// </summary>
        /// <param name="id">The ID of the message to retrieve.</param>
        /// <returns>The message with the specified ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Messages>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null)
                return NotFound();
            return Ok(entity);
        }

        /// <summary>
        /// Creates a new message.
        /// </summary>
        /// <param name="entity">
        /// Used model: Messages (inherits from BaseEntity)
        /// Properties:
        /// - Id (int): Unique identifier for the message (from BaseEntity).
        /// - UserId (string): ID of the user who created the message (from BaseEntity).
        /// - CreatedAt (DateTime): Timestamp of creation (from BaseEntity).
        /// - UpdatedAt (DateTime): Timestamp of last update (from BaseEntity).
        /// - LeadId (int): The ID of the lead associated with the message.
        /// - iceBreakerId (int): The ID of the icebreaker associated with the message.
        /// - Text (string): The content of the message.
        /// - Status (string): The current status of the message (e.g., "New", "Sent", "Delivered", "Read").
        /// </param>
        /// <returns>The created message with its ID.</returns>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Messages entity)
        {
            await _service.AddAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        /// <summary>
        /// Updates an existing message.
        /// </summary>
        /// <param name="id">The ID of the message to update.</param>
        /// <param name="entity">
        /// Used model: Messages (inherits from BaseEntity)
        /// Properties:
        /// - Id (int): Unique identifier for the message (from BaseEntity).
        /// - UserId (string): ID of the user who created the message (from BaseEntity).
        /// - CreatedAt (DateTime): Timestamp of creation (from BaseEntity).
        /// - UpdatedAt (DateTime): Timestamp of last update (from BaseEntity).
        /// - LeadId (int): The ID of the lead associated with the message.
        /// - iceBreakerId (int): The ID of the icebreaker associated with the message.
        /// - Text (string): The content of the message.
        /// - Status (string): The current status of the message (e.g., "New", "Sent", "Delivered", "Read").
        /// </param>
        /// <returns>No content if the update is successful, or BadRequest if ID mismatch.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] Messages entity)
        {
            if (id != entity.Id)
                return BadRequest("ID mismatch");

            await _service.UpdateAsync(entity);
            return NoContent();
        }

        /// <summary>
        /// Deletes a message by its ID.
        /// </summary>
        /// <param name="id">The ID of the message to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Deletes all messages associated with a given entity.
        /// </summary>
        /// <param name="entity">
        /// Used model: Messages (inherits from BaseEntity)
        /// Properties:
        /// - Id (int): Unique identifier for the message (from BaseEntity).
        /// - UserId (string): ID of the user who created the message (from BaseEntity).
        /// - CreatedAt (DateTime): Timestamp of creation (from BaseEntity).
        /// - UpdatedAt (DateTime): Timestamp of last update (from BaseEntity).
        /// - LeadId (int): The ID of the lead associated with the message.
        /// - iceBreakerId (int): The ID of the icebreaker associated with the message.
        /// - Text (string): The content of the message.
        /// - Status (string): The current status of the message (e.g., "New", "Sent", "Delivered", "Read").
        /// </param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAll([FromBody] Messages entity)
        {
            await _service.DeleteAllAsync(entity);
            return NoContent();
        }
    }
}
