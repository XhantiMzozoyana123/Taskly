using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskly.Application.Interfaces;
using Taskly.Domain.Entities;

namespace Taskly.Controllers
{
    /// <summary>
    /// Controller for managing social media login credentials.
    /// Requires authentication for all actions.
    /// </summary>
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

        /// <summary>
        /// Retrieves all social login entries.
        /// </summary>
        /// <returns>A list of all social login entries.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SocialLogins>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a social login entry by its ID.
        /// </summary>
        /// <param name="id">The ID of the social login entry to retrieve.</param>
        /// <returns>The social login entry with the specified ID, or NotFound if not found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<SocialLogins>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null)
                return NotFound();
            return Ok(entity);
        }

        /// <summary>
        /// Creates a new social login entry.
        /// </summary>
        /// <param name="entity">
        /// Used model: SocialLogins (inherits from BaseEntity)
        /// Properties:
        /// - Id (int): Unique identifier for the social login (from BaseEntity).
        /// - UserId (string): ID of the user who owns this social login (from BaseEntity).
        /// - CreatedAt (DateTime): Timestamp of creation (from BaseEntity).
        /// - UpdatedAt (DateTime): Timestamp of last update (from BaseEntity).
        /// - UsernameHash (string): Hashed username for the social media account.
        /// - PasswordHash (string): Hashed password for the social media account.
        /// - Platform (string): The social media platform (e.g., "Reddit", "Twitter", "Facebook").
        /// </param>
        /// <returns>The created social login entry with its ID.</returns>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] SocialLogins entity)
        {
            await _service.AddAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        /// <summary>
        /// Updates an existing social login entry.
        /// </summary>
        /// <param name="id">The ID of the social login entry to update.</param>
        /// <param name="entity">
        /// Used model: SocialLogins (inherits from BaseEntity)
        /// Properties:
        /// - Id (int): Unique identifier for the social login (from BaseEntity).
        /// - UserId (string): ID of the user who owns this social login (from BaseEntity).
        /// - CreatedAt (DateTime): Timestamp of creation (from BaseEntity).
        /// - UpdatedAt (DateTime): Timestamp of last update (from BaseEntity).
        /// - UsernameHash (string): Hashed username for the social media account.
        /// - PasswordHash (string): Hashed password for the social media account.
        /// - Platform (string): The social media platform (e.g., "Reddit", "Twitter", "Facebook").
        /// </param>
        /// <returns>No content if the update is successful, or BadRequest if ID mismatch.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] SocialLogins entity)
        {
            if (id != entity.Id)
                return BadRequest("ID mismatch");

            await _service.UpdateAsync(entity);
            return NoContent();
        }

        /// <summary>
        /// Deletes a social login entry by its ID.
        /// </summary>
        /// <param name="id">The ID of the social login entry to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Deletes all social login entries associated with a given entity.
        /// </summary>
        /// <param name="entity">
        /// Used model: SocialLogins (inherits from BaseEntity)
        /// Properties:
        /// - Id (int): Unique identifier for the social login (from BaseEntity).
        /// - UserId (string): ID of the user who owns this social login (from BaseEntity).
        /// - CreatedAt (DateTime): Timestamp of creation (from BaseEntity).
        /// - UpdatedAt (DateTime): Timestamp of last update (from BaseEntity).
        /// - UsernameHash (string): Hashed username for the social media account.
        /// - PasswordHash (string): Hashed password for the social media account.
        /// - Platform (string): The social media platform (e.g., "Reddit", "Twitter", "Facebook").
        /// </param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAll([FromBody] SocialLogins entity)
        {
            await _service.DeleteAllAsync(entity);
            return NoContent();
        }
    }
}
