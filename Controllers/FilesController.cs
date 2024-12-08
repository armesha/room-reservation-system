using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Services;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

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
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            foreach (var formFile in Request.Form.Files)
            {
                Console.WriteLine($"Form file name: {formFile.Name}, Length: {formFile.Length}");
            }

            if (file == null)
            {
                Console.WriteLine("File is null");
                return BadRequest(new { message = "No file was provided." });
            }

            if (file.Length == 0)
            {
                Console.WriteLine("File length is 0");
                return BadRequest(new { message = "File is empty." });
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                var fileModel = new FileModel
                {
                    FileContent = memoryStream.ToArray()
                };

                _fileService.UploadFile(fileModel);
                Console.WriteLine($"File uploaded successfully. ID: {fileModel.FileId}");
                return Ok(new { id_file = fileModel.FileId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
            }
        }

        // GET: /api/files
        [HttpGet]
        public IActionResult GetFiles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var files = _fileService.GetFiles(page, pageSize);
            var totalCount = _fileService.GetTotalFilesCount();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var fileResponses = files.Select(f => new
            {
                id_file = f.FileId
            });

            return Ok(new
            {
                files = fileResponses,
                currentPage = page,
                pageSize = pageSize,
                totalPages = totalPages,
                totalCount = totalCount
            });
        }

        [HttpPost("clean-duplicates")]
        [Authorize(Roles = "Administrator")]
        [Route("clean-duplicates")]
        public ActionResult<int> CleanDuplicateFiles()
        {
            try
            {
                int deletedCount = _fileService.CleanDuplicateFiles();
                return Ok(new { deletedCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error cleaning duplicate files", error = ex.Message });
            }
        }

        // GET: /api/files/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetFile(int id)
        {
            var file = _fileService.GetFileById(id);
            if (file == null)
                return NotFound(new { message = "File not found." });

            return File(file.FileContent, "image/jpeg");
        }

        // DELETE: /api/files/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteFile(int id)
        {
            try
            {
                var file = _fileService.GetFileById(id);
                if (file == null)
                    return NotFound(new { message = "File not found." });

                _fileService.DeleteFile(id);
                return Ok(new { message = "File successfully deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
            }
        }
    }
}
