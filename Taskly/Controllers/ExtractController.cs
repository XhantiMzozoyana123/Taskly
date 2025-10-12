using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;

namespace Taskly.Controllers
{
    /// <summary>
    /// Controller for handling data extraction operations.
    /// Requires authentication.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExtractController : ControllerBase
    {
        private readonly IExtractService _extractService;

        public ExtractController(IExtractService extractService)
        {
            _extractService = extractService;
        }

        // POST: api/Extract
        /// <summary>
        /// Initiates a data extraction process based on provided search criteria.
        /// Requires 'BasicOrAbove' subscription policy.
        /// </summary>
        /// <param name="searchDto">
        /// Used model: SearchDto
        /// Properties:
        /// - UserId (string): ID of the user making the request.
        /// - Keyword (string): Keyword to search for.
        /// - Query (string): Query to give more context.
        /// - Url (string): URL to filter results.
        /// - Platform (string): Social media platform (e.g., Facebook, Instagram).
        /// - PageNumber (int): For pagination, default to first page.
        /// - PrivateMode (bool): Whether to include private content if permissions allow.
        /// </param>
        /// <returns>An Ok result if the extraction process is initiated successfully, or BadRequest if search criteria is missing.</returns>
        [HttpPost]
        [Authorize(Policy = "BasicOrAbove")]
        public async Task<IActionResult> ExtractAsync([FromBody] SearchDto searchDto)
        {
            if (searchDto == null)
                return BadRequest("Search criteria is required.");

            await _extractService.ExtractAsync(searchDto);

            return Ok();
        }
    }
}
