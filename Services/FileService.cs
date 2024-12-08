using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

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

        public IEnumerable<FileModel> GetFiles(int page, int pageSize)
        {
            return _fileRepository.GetFiles(page, pageSize);
        }

        public int GetTotalFilesCount()
        {
            return _fileRepository.GetTotalFilesCount();
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

        public int CleanDuplicateFiles()
        {
            return _fileRepository.CleanDuplicateFiles();
        }
    }
}
