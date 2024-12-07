using System;

namespace RoomReservationSystem.Models
{
    public class FileResponse
    {
        public int FileId { get; set; }
        public int UploadedBy { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileExtension { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string Operation { get; set; }
    }
}
