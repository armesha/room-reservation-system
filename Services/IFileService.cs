using RoomReservationSystem.Models;
using System.Collections.Generic;

using FileModel = RoomReservationSystem.Models.File;

namespace RoomReservationSystem.Services
{
    public interface IFileService
    {
        IEnumerable<FileModel> GetFiles(int page, int pageSize);
        int GetTotalFilesCount();
        IEnumerable<FileModel> GetAllFilesForUser(int userId);
        FileModel GetFileById(int fileId);
        void UploadFile(FileModel file);
        void DeleteFile(int fileId);
        int CleanDuplicateFiles();
    }
}
