using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Data;
using RoomReservationSystem.Models;
using System.Data;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public EquipmentRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public IEnumerable<Equipment> GetAllEquipment()
        {
            var equipment = new List<Equipment>();

            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT EQUIPMENT_ID, EQUIPMENT_NAME FROM EQUIPMENT ORDER BY EQUIPMENT_NAME";
            command.CommandType = CommandType.Text;

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipment.Add(new Equipment
                {
                    EquipmentId = Convert.ToInt32(reader["EQUIPMENT_ID"]),
                    Name = reader["EQUIPMENT_NAME"].ToString(),
                    Description = null
                });
            }

            return equipment;
        }

        public Equipment GetEquipmentById(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT EQUIPMENT_ID, EQUIPMENT_NAME, DESCRIPTION FROM EQUIPMENT WHERE EQUIPMENT_ID = :equipment_id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("equipment_id", OracleDbType.Int32) { Value = id });

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Equipment
                {
                    EquipmentId = Convert.ToInt32(reader["EQUIPMENT_ID"]),
                    Name = reader["EQUIPMENT_NAME"].ToString(),
                    Description = reader["DESCRIPTION"] == DBNull.Value ? null : reader["DESCRIPTION"].ToString()
                };
            }

            return null;
        }

        public IEnumerable<Equipment> GetEquipmentByRoomId(int roomId)
        {
            var equipment = new List<Equipment>();

            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT e.EQUIPMENT_ID, e.EQUIPMENT_NAME 
                FROM EQUIPMENT e
                JOIN ROOM_EQUIPMENT re ON e.EQUIPMENT_ID = re.EQUIPMENT_ID
                WHERE re.ROOM_ID = :room_id
                ORDER BY e.EQUIPMENT_NAME";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipment.Add(new Equipment
                {
                    EquipmentId = Convert.ToInt32(reader["EQUIPMENT_ID"]),
                    Name = reader["EQUIPMENT_NAME"].ToString(),
                    Description = null
                });
            }

            return equipment;
        }

        public void AddEquipmentToRoom(int roomId, int equipmentId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO room_equipment (room_id, equipment_id) VALUES (:room_id, :equipment_id)";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("equipment_id", OracleDbType.Int32) { Value = equipmentId });

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void RemoveEquipmentFromRoom(int roomId, int equipmentId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM room_equipment WHERE room_id = :room_id AND equipment_id = :equipment_id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("equipment_id", OracleDbType.Int32) { Value = equipmentId });

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void UpdateRoomEquipment(int roomId, List<int> equipmentIds)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Удаляем все текущее оборудование комнаты
                using (var deleteCommand = connection.CreateCommand())
                {
                    deleteCommand.Transaction = transaction;
                    deleteCommand.CommandText = "DELETE FROM room_equipment WHERE room_id = :room_id";
                    deleteCommand.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
                    deleteCommand.ExecuteNonQuery();
                }

                // Добавляем новое оборудование
                if (equipmentIds != null && equipmentIds.Any())
                {
                    using var insertCommand = connection.CreateCommand();
                    insertCommand.Transaction = transaction;
                    insertCommand.CommandText = "INSERT INTO room_equipment (room_id, equipment_id) VALUES (:room_id, :equipment_id)";
                    
                    var roomIdParam = new OracleParameter("room_id", OracleDbType.Int32);
                    var equipmentIdParam = new OracleParameter("equipment_id", OracleDbType.Int32);
                    
                    insertCommand.Parameters.Add(roomIdParam);
                    insertCommand.Parameters.Add(equipmentIdParam);

                    foreach (var equipmentId in equipmentIds)
                    {
                        roomIdParam.Value = roomId;
                        equipmentIdParam.Value = equipmentId;
                        insertCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool IsEquipmentInRoom(int roomId, int equipmentId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM ROOM_EQUIPMENT WHERE ROOM_ID = :room_id AND EQUIPMENT_ID = :equipment_id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("equipment_id", OracleDbType.Int32) { Value = equipmentId });

            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public Equipment UpdateEquipment(Equipment equipment)
        {
            if (GetEquipmentById(equipment.EquipmentId) == null)
            {
                throw new KeyNotFoundException($"Equipment with ID {equipment.EquipmentId} not found");
            }

            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE EQUIPMENT 
                                  SET EQUIPMENT_NAME = :name,
                                      DESCRIPTION = :description 
                                  WHERE EQUIPMENT_ID = :id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("name", OracleDbType.Varchar2) { Value = equipment.Name });
            command.Parameters.Add(new OracleParameter("description", OracleDbType.Varchar2) { Value = (object)equipment.Description ?? DBNull.Value });
            command.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = equipment.EquipmentId });

            connection.Open();
            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException($"Equipment with ID {equipment.EquipmentId} not found");
            }

            return GetEquipmentById(equipment.EquipmentId);
        }

        public Equipment CreateEquipment(Equipment equipment)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO EQUIPMENT 
                                  (EQUIPMENT_ID, EQUIPMENT_NAME, DESCRIPTION) 
                                  VALUES (seq_equipment.NEXTVAL, :name, :description) 
                                  RETURNING EQUIPMENT_ID INTO :id";
            command.CommandType = CommandType.Text;

            var idParameter = new OracleParameter("id", OracleDbType.Int32) { Direction = ParameterDirection.Output };
            command.Parameters.Add(new OracleParameter("name", OracleDbType.Varchar2) { Value = equipment.Name });
            command.Parameters.Add(new OracleParameter("description", OracleDbType.Varchar2) { Value = (object)equipment.Description ?? DBNull.Value });
            command.Parameters.Add(idParameter);

            connection.Open();
            command.ExecuteNonQuery();

            equipment.EquipmentId = Convert.ToInt32(idParameter.Value.ToString());
            return equipment;
        }

        public void DeleteEquipment(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM EQUIPMENT WHERE EQUIPMENT_ID = :id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
