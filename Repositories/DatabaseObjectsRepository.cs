using RoomReservationSystem.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Text.Json;

namespace RoomReservationSystem.Repositories
{
    public class DatabaseObjectsRepository : IDatabaseObjectsRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DatabaseObjectsRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<string>> GetAllTablesAsync()
        {
            var tables = new List<string>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT table_name 
                    FROM user_tables 
                    ORDER BY table_name";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            return tables;
        }

        public async Task<IEnumerable<string>> GetAllDatabaseObjectsAsync()
        {
            var objects = new List<string>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT object_name, object_type 
                    FROM user_objects 
                    WHERE object_type IN ('TABLE', 'VIEW', 'PROCEDURE', 'FUNCTION', 'TRIGGER', 'PACKAGE')
                    ORDER BY object_type, object_name";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    objects.Add($"{reader.GetString(1)}: {reader.GetString(0)}");
                }
            }
            return objects;
        }

        public async Task<IEnumerable<dynamic>> GetTableColumnsAsync(string tableName)
        {
            var columns = new List<dynamic>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT column_name, data_type, data_length, nullable
                    FROM user_tab_columns 
                    WHERE table_name = :tableName 
                    ORDER BY column_id";

                var param = command.CreateParameter();
                param.ParameterName = ":tableName";
                param.Value = tableName.ToUpper();
                command.Parameters.Add(param);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new
                    {
                        ColumnName = reader.GetString(0),
                        DataType = reader.GetString(1),
                        Length = reader.GetInt32(2),
                        IsNullable = reader.GetString(3) == "Y"
                    });
                }
            }
            return columns;
        }

        public async Task<(IEnumerable<dynamic> Data, int TotalCount)> GetTableDataAsync(string tableName, int limit = 10, int offset = 0)
        {
            var data = new List<dynamic>();
            int totalCount = 0;

            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                // Get total count
                using (var countCommand = connection.CreateCommand())
                {
                    countCommand.CommandText = $"SELECT COUNT(*) FROM {tableName}";
                    totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }

                // Get data with pagination
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"
                        SELECT * FROM (
                            SELECT a.*, ROWNUM rnum FROM (
                                SELECT * FROM {tableName}
                            ) a WHERE ROWNUM <= :maxRow
                        ) WHERE rnum > :minRow";

                    var maxRowParam = command.CreateParameter();
                    maxRowParam.ParameterName = ":maxRow";
                    maxRowParam.Value = offset + limit;
                    command.Parameters.Add(maxRowParam);

                    var minRowParam = command.CreateParameter();
                    minRowParam.ParameterName = ":minRow";
                    minRowParam.Value = offset;
                    command.Parameters.Add(minRowParam);

                    using var reader = await command.ExecuteReaderAsync();
                    var columns = Enumerable.Range(0, reader.FieldCount)
                        .Select(i => reader.GetName(i))
                        .Where(name => name != "RNUM")
                        .ToList();

                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        foreach (var column in columns)
                        {
                            row[column] = reader[column] == DBNull.Value ? null : reader[column];
                        }
                        data.Add(row);
                    }
                }
            }

            return (data, totalCount);
        }

        private OracleDbType GetOracleType(string dataType)
        {
            return dataType.ToUpper() switch
            {
                "NUMBER" => OracleDbType.Decimal,
                "VARCHAR2" => OracleDbType.Varchar2,
                "CHAR" => OracleDbType.Char,
                "DATE" => OracleDbType.Date,
                "BLOB" => OracleDbType.Blob,
                "CLOB" => OracleDbType.Clob,
                _ => OracleDbType.Varchar2
            };
        }

        private object ConvertToOracleValue(object value, string dataType)
        {
            if (value == null) return DBNull.Value;

            return dataType.ToUpper() switch
            {
                "NUMBER" => value is JsonElement jsonNum 
                    ? jsonNum.ValueKind == JsonValueKind.String 
                        ? decimal.Parse(jsonNum.GetString()) 
                        : jsonNum.GetDecimal()
                    : Convert.ToDecimal(value),

                "VARCHAR2" or "CHAR" => value is JsonElement jsonStr 
                    ? jsonStr.GetString() 
                    : value.ToString(),

                "DATE" => value is JsonElement jsonDate 
                    ? DateTime.Parse(jsonDate.GetString()) 
                    : value is string strDate 
                        ? DateTime.Parse(strDate) 
                        : Convert.ToDateTime(value),

                "BLOB" => value is JsonElement jsonBlob 
                    ? Convert.FromBase64String(jsonBlob.GetString()) 
                    : value is string strBlob 
                        ? Convert.FromBase64String(strBlob) 
                        : value,

                "CLOB" => value is JsonElement jsonClob 
                    ? jsonClob.GetString() 
                    : value.ToString(),

                _ => value.ToString()
            };
        }

        public async Task<string> SaveTableDataAsync(string tableName, Dictionary<string, object> data)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                
                // Get parameters list in correct order
                var columns = await GetTableColumnsAsync(tableName);
                var paramList = new List<string>();
                
                // Get procedure parameters list
                var procedureName = $"edit_pkg.edit_{tableName.ToLower()}";
                var procParams = await GetProcedureParametersAsync(connection, procedureName);
                
                // Form parameter list for SQL query
                foreach (var column in columns)
                {
                    // Skip columns that are not in the procedure
                    var paramName = $"p_{column.ColumnName.ToLower()}";
                    if (!procParams.Contains(paramName))
                    {
                        continue;
                    }

                    paramName = $":{paramName}";
                    paramList.Add(paramName);
                    
                    var param = command.CreateParameter();
                    param.ParameterName = paramName;
                    
                    // Search for value case-insensitively
                    var columnNameLower = column.ColumnName.ToLower();
                    var value = data.FirstOrDefault(x => x.Key.ToLower() == columnNameLower).Value;

                    // Set parameter type and value
                    param.OracleDbType = GetOracleType(column.DataType);
                    if (param.OracleDbType == OracleDbType.Varchar2 || param.OracleDbType == OracleDbType.Char)
                    {
                        param.Size = column.Length;
                    }

                    try
                    {
                        param.Value = ConvertToOracleValue(value, column.DataType);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting value for parameter {paramName}: {ex.Message}");
                        param.Value = DBNull.Value;
                    }
                    
                    command.Parameters.Add(param);
                    Console.WriteLine($"Parameter: {paramName}, Value: {value}, Type: {column.DataType}");
                }

                // Add output parameter
                var resultParamName = ":p_result";
                paramList.Add(resultParamName);
                var resultParam = command.CreateParameter();
                resultParam.ParameterName = resultParamName;
                resultParam.Direction = ParameterDirection.Output;
                resultParam.OracleDbType = OracleDbType.Varchar2;
                resultParam.Size = 4000;
                command.Parameters.Add(resultParam);

                // Form SQL query without BEGIN/END block
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = procedureName;
                Console.WriteLine($"Executing procedure: {command.CommandText}");

                await command.ExecuteNonQueryAsync();
                return resultParam.Value?.ToString() ?? "Success";
            }
        }

        private async Task<HashSet<string>> GetProcedureParametersAsync(OracleConnection connection, string procedureName)
        {
            var parameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using var command = connection.CreateCommand();
            
            // Split package and procedure name
            var nameParts = procedureName.Split('.');
            var packageName = nameParts.Length > 1 ? nameParts[0] : null;
            var procName = nameParts.Length > 1 ? nameParts[1] : procedureName;
            
            command.CommandText = @"
                SELECT argument_name
                FROM user_arguments 
                WHERE object_name = :procName
                AND (:packageName IS NULL OR package_name = :packageName)
                AND argument_name IS NOT NULL
                ORDER BY position";

            var procParam = command.CreateParameter();
            procParam.ParameterName = ":procName";
            procParam.Value = procName.ToUpper();
            command.Parameters.Add(procParam);

            var pkgParam = command.CreateParameter();
            pkgParam.ParameterName = ":packageName";
            pkgParam.Value = packageName?.ToUpper() ?? (object)DBNull.Value;
            command.Parameters.Add(pkgParam);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                parameters.Add(reader.GetString(0).ToLower());
            }
            return parameters;
        }
    }
}
