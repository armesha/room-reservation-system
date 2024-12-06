// Repositories/IFileRepository.cs
using RoomReservationSystem.Models;
using System.Collections.Generic;

// Alias for the custom File model
using FileModel = RoomReservationSystem.Models.File;

namespace RoomReservationSystem.Repositories
{
    public interface IFileRepository
    {
        IEnumerable<FileModel> GetAllFilesForUser(int userId);
        FileModel GetFileById(int fileId);
        void AddFile(FileModel file);
        void DeleteFile(int fileId);
    }
}
