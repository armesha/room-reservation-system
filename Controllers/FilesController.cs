using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Services;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

// Alias for the custom File model
using FileModel = RoomReservationSystem.Models.File;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Authorize(Roles = "Administrator,Registered User")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // POST: /api/files/upload
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile File)
        {
            Console.WriteLine("Content-Type of request: " + Request.ContentType);
            Console.WriteLine("Total files received: " + Request.Form.Files.Count);

            foreach (var file in Request.Form.Files)
            {
                Console.WriteLine("File name: " + file.FileName);
            }

            if (File == null || File.Length == 0)
            {
                Console.WriteLine("No file uploaded.");
                return BadRequest(new { message = "No file uploaded." });
            }

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            using var memoryStream = new MemoryStream();
            await File.CopyToAsync(memoryStream);

            var fileModel = new FileModel
            {
                UploadedBy = userId,
                FileName = Path.GetFileName(File.FileName),
                FileType = File.ContentType,
                FileExtension = Path.GetExtension(File.FileName),
                UploadDate = DateTime.UtcNow,
                Operation = "Upload",
                FileContent = memoryStream.ToArray()
            };

            _fileService.UploadFile(fileModel);
            return Ok(new { file = fileModel });
        }

        // GET: /api/files/{id}
        [HttpGet("{id}")]
        public IActionResult GetFile(int id)
        {
            var file = _fileService.GetFileById(id);
            if (file == null)
                return NotFound(new { message = "File not found." });

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Administrator" && file.UploadedBy != userId)
                return Forbid();

            // Return the file as a downloadable content
            return File(file.FileContent, file.FileType, file.FileName);
        }

        // DELETE: /api/files/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteFile(int id)
        {
            var file = _fileService.GetFileById(id);
            if (file == null)
                return NotFound(new { message = "File not found." });

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Administrator" && file.UploadedBy != userId)
                return Forbid();

            _fileService.DeleteFile(id);
            return NoContent();
        }
    }
}
