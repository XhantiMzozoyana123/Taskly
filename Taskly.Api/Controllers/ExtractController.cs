using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;

namespace Taskly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtractController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IExtractService _extractService;
        private readonly IUiLogger _logger;

        public ExtractController(IExtractService extractService, IUiLogger logger, ApplicationDbContext context)
        {
            _extractService = extractService;
            _logger = logger;

            _context = context;
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
                var settings = await _context.Settings.FirstOrDefaultAsync();
                var httpMode = settings.DomainRotateWhenExtractingOnline;
                
                // Start extraction
                if (httpMode)
                {
                    var domains = await _context.Domains.ToListAsync();

                    int domainCount = domains.Count;
                    int domainIndex = 0; // tracks which domain to use next

                    for (int i = 0; i < searchDtos.Count; i++)
                    {
                        var searchDto = searchDtos[i];
                        searchDto.PrivateMode = true;

                        // Round-robin selection
                        var selectedDomain = domains[domainIndex % domains.Count];
                        var domainUrl = selectedDomain.Url;

                        var cookieRotate = settings.CookieRotateWhenExtractingOnline;
                        if (cookieRotate)
                        {
                            var cookieFile = await _context.CookieFiles.ToListAsync();

                            var random = new Random();
                            int number = random.Next(0, cookieFile.Count);

                            searchDto.CookiePath = cookieFile[number].FileName;
                        }

                        // Perform extraction
                        ApiConstant.ExtractorHttpRequest(searchDto, domainUrl);

                        // Move to next domain
                        domainIndex++;
                    }

                }
                else
                {
                    foreach (var searchDto in searchDtos)
                    {
                        searchDto.PrivateMode = true;
                        await _extractService.ExtractAsync(searchDto);
                    }
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
