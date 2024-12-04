// Models/FileUploadRequest.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class FileUploadRequest
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
