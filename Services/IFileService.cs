// Services/IFileService.cs
using RoomReservationSystem.Models;
using System.Collections.Generic;

// Alias for the custom File model
using FileModel = RoomReservationSystem.Models.File;

namespace RoomReservationSystem.Services
{
    public interface IFileService
    {
        IEnumerable<FileModel> GetAllFilesForUser(int userId);
        FileModel GetFileById(int fileId);
        void UploadFile(FileModel file);
        void DeleteFile(int fileId);
    }
}
