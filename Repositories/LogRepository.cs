using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoomReservationSystem.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly string _connectionString;

        public LogRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Log> GetAllLogs()
        {
            var logs = new List<Log>();
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT LOG_ID, USERNAME, ACTION, LOG_DATE, TABLE_NAME FROM LOGS ORDER BY LOG_DATE DESC";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(MapLog(reader));
                        }
                    }
                }
            }
            return logs;
        }

        public Log GetLogById(int logId)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT LOG_ID, USERNAME, ACTION, LOG_DATE, TABLE_NAME FROM LOGS WHERE LOG_ID = :logId";
                    cmd.Parameters.Add(":logId", OracleDbType.Int32).Value = logId;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapLog(reader);
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<Log> GetLogsByUsername(string username)
        {
            var logs = new List<Log>();
            using (var conn = new OracleConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT LOG_ID, USERNAME, ACTION, LOG_DATE, TABLE_NAME FROM LOGS WHERE USERNAME = :username ORDER BY LOG_DATE DESC";
                    cmd.Parameters.Add(":username", OracleDbType.Varchar2).Value = username;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(MapLog(reader));
                        }
                    }
                }
            }
            return logs;
        }

        private Log MapLog(OracleDataReader reader)
        {
            return new Log
            {
                LogId = reader.GetInt32(reader.GetOrdinal("LOG_ID")),
                Username = reader.IsDBNull(reader.GetOrdinal("USERNAME")) ? null : reader.GetString(reader.GetOrdinal("USERNAME")),
                Action = reader.IsDBNull(reader.GetOrdinal("ACTION")) ? null : reader.GetString(reader.GetOrdinal("ACTION")),
                LogDate = reader.GetDateTime(reader.GetOrdinal("LOG_DATE")),
                TableName = reader.IsDBNull(reader.GetOrdinal("TABLE_NAME")) ? null : reader.GetString(reader.GetOrdinal("TABLE_NAME"))
            };
        }
    }
}
