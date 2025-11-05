using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Taskly.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public FileUploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Upload folder inside wwwroot
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Use the original file name
            string fileName = Path.GetFileName(file.FileName); // gets only the file name, strips any path
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Optional: make unique if file exists
            int count = 1;
            string nameOnly = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            while (System.IO.File.Exists(filePath))
            {
                fileName = $"{nameOnly}({count}){extension}";
                filePath = Path.Combine(uploadsFolder, fileName);
                count++;
            }

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return **relative path** with forward slashes
            string relativePath = Path.Combine("uploads", fileName).Replace("\\", "/");
            return Ok(new { filePath = relativePath });
        }

    }
}
