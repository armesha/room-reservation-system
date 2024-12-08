using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Data;
using RoomReservationSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace RoomReservationSystem.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public User GetUserByUsername(string username)
        {
            int maxRetries = 3;
            int currentRetry = 0;
            int delayMs = 1000;

            while (currentRetry < maxRetries)
            {
                try
                {
                    using var connection = _connectionFactory.CreateConnection();
                    connection.Open();
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT user_id, username, password_hash, email, role_id, registration_date 
                        FROM users 
                        WHERE username = :username";
                    command.Parameters.Add(new OracleParameter("username", OracleDbType.Varchar2) { Value = username });

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        return new User
                        {
                            UserId = Convert.ToInt32(reader["user_id"]),
                            Username = reader["username"].ToString(),
                            PasswordHash = reader["password_hash"].ToString(),
                            Email = reader["email"].ToString(),
                            RoleId = Convert.ToInt32(reader["role_id"]),
                            RegistrationDate = Convert.ToDateTime(reader["registration_date"])
                        };
                    }
                    return null;
                }
                catch (OracleException ex) when (ex.Number == 1033 || ex.Number == 1034 || ex.Number == 1035)
                {
                    currentRetry++;
                    if (currentRetry < maxRetries)
                    {
                        Thread.Sleep(delayMs);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return null;
        }

        public User GetUserByEmail(string email)
        {
            int maxRetries = 3;
            int currentRetry = 0;
            int delayMs = 1000;

            while (currentRetry < maxRetries)
            {
                try
                {
                    using var connection = _connectionFactory.CreateConnection();
                    connection.Open();
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT user_id, username, password_hash, email, role_id, registration_date 
                        FROM users 
                        WHERE email = :email";
                    command.Parameters.Add(new OracleParameter("email", OracleDbType.Varchar2) { Value = email });

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        return new User
                        {
                            UserId = Convert.ToInt32(reader["user_id"]),
                            Username = reader["username"].ToString(),
                            PasswordHash = reader["password_hash"].ToString(),
                            Email = reader["email"].ToString(),
                            RoleId = Convert.ToInt32(reader["role_id"]),
                            RegistrationDate = Convert.ToDateTime(reader["registration_date"])
                        };
                    }
                    return null;
                }
                catch (OracleException ex)
                {
                    currentRetry++;
                    if (currentRetry == maxRetries)
                        throw;
                    Thread.Sleep(delayMs * currentRetry);
                }
            }
            return null;
        }

        public User GetUserById(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT u.user_id, u.username, u.password_hash, u.email, 
                     u.role_id, u.registration_date, u.name, u.surname, 
                     u.phone, u.code, r.role_name
                  FROM users u
                  LEFT JOIN roles r ON u.role_id = r.role_id
                  WHERE u.user_id = :user_id";
            command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = userId });

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = Convert.ToInt32(reader["user_id"]),
                    Username = reader["username"].ToString(),
                    PasswordHash = reader["password_hash"].ToString(),
                    Email = reader["email"].ToString(),
                    RoleId = Convert.ToInt32(reader["role_id"]),
                    RegistrationDate = Convert.ToDateTime(reader["registration_date"]),
                    Name = reader["name"] != DBNull.Value ? reader["name"].ToString() : null,
                    Surname = reader["surname"] != DBNull.Value ? reader["surname"].ToString() : null,
                    Phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : null,
                    Code = reader["code"] != DBNull.Value ? reader["code"].ToString() : null,
                    RoleName = reader["role_name"].ToString()
                };
            }
            return null;
        }

        public IEnumerable<User> GetAllUsers()
        {
            var users = new List<User>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT user_id, username, password_hash, email, role_id, registration_date 
                FROM users";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    UserId = Convert.ToInt32(reader["user_id"]),
                    Username = reader["username"].ToString(),
                    PasswordHash = reader["password_hash"].ToString(),
                    Email = reader["email"].ToString(),
                    RoleId = Convert.ToInt32(reader["role_id"]),
                    RegistrationDate = Convert.ToDateTime(reader["registration_date"])
                });
            }
            return users;
        }

        public IEnumerable<User> GetPaginatedUsers(UserFilterParameters parameters)
        {
            var users = new List<User>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT u.*, r.role_name
                FROM (
                    SELECT a.*, ROWNUM rnum
                    FROM (
                        SELECT u.user_id, u.username, u.password_hash, u.email, u.role_id, 
                               u.registration_date, u.name, u.surname, u.phone, u.code
                        FROM users u
                        ORDER BY u.user_id
                    ) a
                    WHERE ROWNUM <= :end_row
                ) u
                JOIN roles r ON u.role_id = r.role_id
                WHERE rnum > :start_row";

            command.Parameters.Add(new OracleParameter("end_row", OracleDbType.Int32) { Value = parameters.Offset + parameters.Count });
            command.Parameters.Add(new OracleParameter("start_row", OracleDbType.Int32) { Value = parameters.Offset });

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    UserId = Convert.ToInt32(reader["user_id"]),
                    Username = reader["username"].ToString(),
                    PasswordHash = reader["password_hash"].ToString(),
                    Email = reader["email"].ToString(),
                    RoleId = Convert.ToInt32(reader["role_id"]),
                    RoleName = reader["role_name"].ToString(),
                    RegistrationDate = Convert.ToDateTime(reader["registration_date"]),
                    Name = reader["name"] == DBNull.Value ? null : reader["name"].ToString(),
                    Surname = reader["surname"] == DBNull.Value ? null : reader["surname"].ToString(),
                    Phone = reader["phone"] == DBNull.Value ? null : reader["phone"].ToString(),
                    Code = reader["code"] == DBNull.Value ? null : reader["code"].ToString()
                });
            }
            return users;
        }

        public int GetTotalUsersCount()
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM users";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public void AddUser(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            using (var seqCommand = connection.CreateCommand())
            {
                seqCommand.CommandText = "SELECT seq_users.NEXTVAL FROM DUAL";
                user.UserId = Convert.ToInt32(seqCommand.ExecuteScalar());
            }

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO users 
                (user_id, username, password_hash, email, role_id, registration_date) 
                VALUES 
                (:user_id, :username, :password_hash, :email, :role_id, :registration_date)";

            command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = user.UserId });
            command.Parameters.Add(new OracleParameter("username", OracleDbType.Varchar2) { Value = user.Username });
            command.Parameters.Add(new OracleParameter("password_hash", OracleDbType.Varchar2) { Value = user.PasswordHash });
            command.Parameters.Add(new OracleParameter("email", OracleDbType.Varchar2) { Value = user.Email });
            command.Parameters.Add(new OracleParameter("role_id", OracleDbType.Int32) { Value = user.RoleId });
            command.Parameters.Add(new OracleParameter("registration_date", OracleDbType.Date) { Value = user.RegistrationDate });

            command.ExecuteNonQuery();
        }

        public User UpdateUser(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE users 
                SET username = :username, 
                    email = :email,
                    role_id = :role_id,
                    name = :name,
                    surname = :surname,
                    phone = :phone,
                    code = :code,
                    password_hash = :password_hash
                WHERE user_id = :user_id";

            command.Parameters.Add(new OracleParameter("username", OracleDbType.Varchar2) { Value = user.Username });
            command.Parameters.Add(new OracleParameter("email", OracleDbType.Varchar2) { Value = user.Email });
            command.Parameters.Add(new OracleParameter("role_id", OracleDbType.Int32) { Value = user.RoleId });
            command.Parameters.Add(new OracleParameter("name", OracleDbType.Varchar2) { Value = (object)user.Name ?? DBNull.Value });
            command.Parameters.Add(new OracleParameter("surname", OracleDbType.Varchar2) { Value = (object)user.Surname ?? DBNull.Value });
            command.Parameters.Add(new OracleParameter("phone", OracleDbType.Varchar2) { Value = (object)user.Phone ?? DBNull.Value });
            command.Parameters.Add(new OracleParameter("code", OracleDbType.Varchar2) { Value = (object)user.Code ?? DBNull.Value });
            command.Parameters.Add(new OracleParameter("password_hash", OracleDbType.Varchar2) { Value = user.PasswordHash });
            command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = user.UserId });

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0 ? GetUserById(user.UserId) : null;
        }

        public void DeleteUser(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // First, delete related logs
                using var deleteLogsCommand = connection.CreateCommand();
                deleteLogsCommand.Transaction = transaction;
                deleteLogsCommand.CommandText = @"
                    DELETE FROM logs 
                    WHERE user_id = :user_id";
                deleteLogsCommand.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = userId });
                deleteLogsCommand.ExecuteNonQuery();

                // Then, delete the user
                using var deleteUserCommand = connection.CreateCommand();
                deleteUserCommand.Transaction = transaction;
                deleteUserCommand.CommandText = @"
                    DELETE FROM users 
                    WHERE user_id = :user_id";
                deleteUserCommand.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = userId });
                int rowsAffected = deleteUserCommand.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("User not found or already deleted.");
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
