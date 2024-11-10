// Services/FileService.cs
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

// Alias for the custom File model
using FileModel = RoomReservationSystem.Models.File;

namespace RoomReservationSystem.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;

        public FileService(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public IEnumerable<FileModel> GetAllFilesForUser(int userId)
        {
            return _fileRepository.GetAllFilesForUser(userId);
        }

        public FileModel GetFileById(int fileId)
        {
            return _fileRepository.GetFileById(fileId);
        }

        public void UploadFile(FileModel file)
        {
            _fileRepository.AddFile(file);
        }

        public void DeleteFile(int fileId)
        {
            _fileRepository.DeleteFile(fileId);
        }
    }
}
