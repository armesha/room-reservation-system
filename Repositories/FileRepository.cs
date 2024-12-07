using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Data;
using RoomReservationSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;

// Alias for the custom File model
using FileModel = RoomReservationSystem.Models.File;

namespace RoomReservationSystem.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public FileRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<FileModel> GetFiles(int page, int pageSize)
        {
            var files = new List<FileModel>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            
            // Упрощенный запрос без пагинации для отладки
            command.CommandText = "SELECT ID_FILE FROM FILES ORDER BY ID_FILE DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var fileId = Convert.ToInt32(reader["ID_FILE"]);
                Console.WriteLine($"Found file with ID: {fileId}"); // Логирование
                files.Add(new FileModel { FileId = fileId });
            }

            Console.WriteLine($"Total files found: {files.Count}"); // Логирование
            return files;
        }

        public int GetTotalFilesCount()
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM FILES";
            var count = Convert.ToInt32(command.ExecuteScalar());
            Console.WriteLine($"Total count from database: {count}"); // Логирование
            return count;
        }

        public IEnumerable<FileModel> GetAllFilesForUser(int userId)
        {
            var files = new List<FileModel>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT ID_FILE, IMAGE 
                                    FROM FILES 
                                    WHERE ID_FILE = :user_id";
            command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = userId });

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                files.Add(new FileModel
                {
                    FileId = Convert.ToInt32(reader["ID_FILE"]),
                    FileContent = (byte[])reader["IMAGE"]
                });
            }
            return files;
        }

        public FileModel GetFileById(int fileId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT ID_FILE, IMAGE 
                                    FROM FILES WHERE ID_FILE = :file_id";
            command.Parameters.Add(new OracleParameter("file_id", OracleDbType.Int32) { Value = fileId });

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new FileModel
                {
                    FileId = Convert.ToInt32(reader["ID_FILE"]),
                    FileContent = reader["IMAGE"] as byte[]
                };
            }
            return null;
        }

        public void AddFile(FileModel file)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"INSERT INTO FILES 
                                        (ID_FILE, IMAGE) 
                                        VALUES 
                                        (SEQ_FILES.NEXTVAL, :file_content)
                                        RETURNING ID_FILE INTO :id_file";
                
                command.Parameters.Add(new OracleParameter("file_content", OracleDbType.Blob) { Value = file.FileContent });
                var idParam = new OracleParameter("id_file", OracleDbType.Decimal);
                idParam.Direction = System.Data.ParameterDirection.Output;
                command.Parameters.Add(idParam);

                command.ExecuteNonQuery();
                
                // Правильное преобразование OracleDecimal в int
                if (idParam.Value != null && idParam.Value != DBNull.Value)
                {
                    var oracleDecimal = (Oracle.ManagedDataAccess.Types.OracleDecimal)idParam.Value;
                    file.FileId = (int)oracleDecimal.Value;
                }
                
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void DeleteFile(int fileId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"DELETE FROM FILES WHERE ID_FILE = :file_id";
            command.Parameters.Add(new OracleParameter("file_id", OracleDbType.Int32) { Value = fileId });

            command.ExecuteNonQuery();
        }
    }
}
