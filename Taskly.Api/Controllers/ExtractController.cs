using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;

namespace Taskly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtractController : ControllerBase
    {
        private readonly IExtractService _extractService;
        private readonly IUiLogger _logger;

        public ExtractController(IExtractService extractService, IUiLogger logger)
        {
            _extractService = extractService;
            _logger = logger;
        }

        /// <summary>
        /// Initiates multi-platform or single-platform extraction based on SearchDto.
        /// </summary>
        /// <param name="searchDto">Search parameters, keyword, and cookie paths.</param>
        /// <returns>HTTP response with extraction status.</returns>
        [HttpPost("start")]
        public async Task<IActionResult> StartExtraction([FromBody] SearchDto searchDto)
        {
            if (searchDto == null)
                return BadRequest("Search parameters cannot be null.");

            if (string.IsNullOrWhiteSpace(searchDto.Keyword))
                return BadRequest("Search keyword is required.");

            try
            {
                _logger.LogInfo($"Received extraction request for keyword: '{searchDto.Keyword}' (MultiPlatform: {searchDto.MultiPlatform})");

                searchDto.PrivateMode = true;

                // Start extraction
                await _extractService.ExtractAsync(searchDto);

                _logger.LogInfo("Extraction completed successfully.");
                return Ok(new
                {
                    success = true,
                    message = "Extraction completed successfully."
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Invalid request: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error during extraction: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "An internal error occurred during extraction.",
                    details = ex.Message
                });
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> StartBatchExtraction([FromBody] List<SearchDto> searchDtos)
        {
            if (searchDtos == null)
                return BadRequest("Search parameters cannot be null.");

            try
            {
                // All extraction runs locally on this machine — no remote/VPS requests.
                foreach (var searchDto in searchDtos)
                {
                    searchDto.PrivateMode = true;
                    await _extractService.ExtractAsync(searchDto);
                }

                _logger.LogInfo("Extraction completed successfully.");
                return Ok(new
                {
                    success = true,
                    message = "Extraction completed successfully."
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Invalid request: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error during extraction: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "An internal error occurred during extraction.",
                    details = ex.Message
                });
            }
        }
    }
}
