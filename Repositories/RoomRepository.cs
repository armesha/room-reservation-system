using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Data;
using RoomReservationSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;

namespace RoomReservationSystem.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public RoomRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public void AddRoom(Room room)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction();

            try
            {
                // Insert into rooms table
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO rooms (room_id, building_id, room_number, capacity, description, id_file, price) 
                    VALUES (seq_rooms.NEXTVAL, :building_id, :room_number, :capacity, :description, :id_file, :price) 
                    RETURNING room_id INTO :room_id";

                command.CommandType = CommandType.Text;

                var roomIdParam = new OracleParameter("room_id", OracleDbType.Int32);
                roomIdParam.Direction = ParameterDirection.Output;

                command.Parameters.Add(new OracleParameter("building_id", OracleDbType.Int32) { Value = room.BuildingId });
                command.Parameters.Add(new OracleParameter("room_number", OracleDbType.Varchar2) { Value = room.RoomNumber });
                command.Parameters.Add(new OracleParameter("capacity", OracleDbType.Int32) { Value = room.Capacity });
                command.Parameters.Add(new OracleParameter("description", OracleDbType.Varchar2) { Value = room.Description ?? string.Empty });
                command.Parameters.Add(new OracleParameter("id_file", OracleDbType.Int32) { Value = room.IdFile.HasValue ? (object)room.IdFile.Value : DBNull.Value });
                command.Parameters.Add(new OracleParameter("price", OracleDbType.Decimal) { Value = room.Price });
                command.Parameters.Add(roomIdParam);

                command.ExecuteNonQuery();

                int newRoomId = Convert.ToInt32(roomIdParam.Value.ToString());

                // Insert equipment associations
                if (room.Equipment != null && room.Equipment.Any())
                {
                    var equipmentCommand = connection.CreateCommand();
                    equipmentCommand.Transaction = transaction;
                    equipmentCommand.CommandText = "INSERT INTO room_equipment (room_id, equipment_id) VALUES (:room_id, :equipment_id)";
                    equipmentCommand.CommandType = CommandType.Text;

                    var roomIdEquipParam = new OracleParameter("room_id", OracleDbType.Int32);
                    var equipmentIdParam = new OracleParameter("equipment_id", OracleDbType.Int32);

                    equipmentCommand.Parameters.Add(roomIdEquipParam);
                    equipmentCommand.Parameters.Add(equipmentIdParam);

                    foreach (var equipment in room.Equipment)
                    {
                        roomIdEquipParam.Value = newRoomId;
                        equipmentIdParam.Value = equipment.EquipmentId;
                        equipmentCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                room.RoomId = newRoomId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        public IEnumerable<Room> GetAllRooms(int? limit = null, int? offset = null, RoomFilterParameters filters = null)
        {
            var rooms = new List<Room>();
            var connection = _dbConnectionFactory.CreateConnection();

            // Step 1: Get room IDs with filtering
            var command = connection.CreateCommand();
            var sql = @"
                SELECT DISTINCT r.room_id
                FROM rooms r
                LEFT JOIN room_equipment re ON r.room_id = re.room_id
                WHERE 1=1";

            var parameters = new List<OracleParameter>();

            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.Name))
                {
                    var searchName = filters.Name.Replace("+", " ").Trim();
                    sql += " AND (LOWER(r.room_number) LIKE LOWER(:p_name) OR LOWER(r.description) LIKE LOWER(:p_name))";
                    parameters.Add(new OracleParameter("p_name", OracleDbType.Varchar2) { Value = $"%{searchName}%" });
                }

                if (filters.MinPrice.HasValue)
                {
                    sql += " AND r.price >= :p_min_price";
                    parameters.Add(new OracleParameter("p_min_price", OracleDbType.Decimal) { Value = filters.MinPrice.Value * 0.8m });
                }

                if (filters.MaxPrice.HasValue)
                {
                    sql += " AND r.price <= :p_max_price";
                    parameters.Add(new OracleParameter("p_max_price", OracleDbType.Decimal) { Value = filters.MaxPrice.Value * 1.2m });
                }

                if (filters.MinCapacity.HasValue)
                {
                    sql += " AND r.capacity >= :p_min_capacity";
                    parameters.Add(new OracleParameter("p_min_capacity", OracleDbType.Int32) { Value = (int)(filters.MinCapacity.Value * 0.8) });
                }

                if (filters.MaxCapacity.HasValue)
                {
                    sql += " AND r.capacity <= :p_max_capacity";
                    parameters.Add(new OracleParameter("p_max_capacity", OracleDbType.Int32) { Value = (int)(filters.MaxCapacity.Value * 1.2) });
                }

                if (filters.BuildingId.HasValue)
                {
                    sql += " AND r.building_id = :p_building_id";
                    parameters.Add(new OracleParameter("p_building_id", OracleDbType.Int32) { Value = filters.BuildingId.Value });
                }

                if (filters.EquipmentIds != null && filters.EquipmentIds.Any())
                {
                    sql += @" AND r.room_id IN (
                        SELECT room_id 
                        FROM room_equipment 
                        WHERE equipment_id IN (" + string.Join(",", filters.EquipmentIds.Select((_, i) => $":equipment_id_{i}")) + @")
                        GROUP BY room_id 
                        HAVING COUNT(*) = :p_equipment_count
                    )";

                    for (int i = 0; i < filters.EquipmentIds.Count; i++)
                    {
                        parameters.Add(new OracleParameter($"equipment_id_{i}", OracleDbType.Int32) { Value = filters.EquipmentIds[i] });
                    }
                    parameters.Add(new OracleParameter("p_equipment_count", OracleDbType.Int32) { Value = filters.EquipmentIds.Count });
                }
            }

            sql += " ORDER BY r.room_id";

            if (offset.HasValue)
            {
                sql += " OFFSET :p_offset ROWS";
                parameters.Add(new OracleParameter("p_offset", OracleDbType.Int32) { Value = offset.Value });
            }
            if (limit.HasValue)
            {
                sql += " FETCH NEXT :p_limit ROWS ONLY";
                parameters.Add(new OracleParameter("p_limit", OracleDbType.Int32) { Value = limit.Value });
            }

            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            connection.Open();
            var roomIds = new List<int>();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                roomIds.Add(Convert.ToInt32(reader["room_id"]));
            }
            reader.Close();

            // Step 2: Get full room data for each ID
            foreach (var roomId in roomIds)
            {
                var room = GetRoomById(roomId);
                if (room != null)
                {
                    rooms.Add(room);
                }
            }
            connection.Close();

            return rooms;
        }

        public IEnumerable<Room> GetRandomRooms(int count)
        {
            var rooms = new List<Room>();

            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM (SELECT * FROM rooms ORDER BY DBMS_RANDOM.VALUE) WHERE ROWNUM <= :count";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new OracleParameter("count", OracleDbType.Int32) { Value = count });

            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rooms.Add(MapRoomFromReader(reader));
            }
            reader.Close();
            connection.Close();

            return rooms;
        }

        public IEnumerable<DateTime> GetReservedDates(int roomId, DateTime startDate, DateTime endDate)
        {
            var reservedDates = new List<DateTime>();

            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT DISTINCT TRUNC(start_time) as reserved_date
                FROM bookings
                WHERE room_id = :roomId 
                AND TRUNC(start_time) BETWEEN TRUNC(:startDate) AND TRUNC(:endDate)
                ORDER BY reserved_date";

            command.CommandType = CommandType.Text;
            command.Parameters.Add(new OracleParameter("roomId", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("startDate", OracleDbType.Date) { Value = startDate });
            command.Parameters.Add(new OracleParameter("endDate", OracleDbType.Date) { Value = endDate });

            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                reservedDates.Add(Convert.ToDateTime(reader["reserved_date"]));
            }
            reader.Close();
            connection.Close();

            return reservedDates;
        }

        public Room GetRoomById(int roomId)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM rooms WHERE room_id = :room_id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var room = MapRoomFromReader(reader);
                reader.Close();
                connection.Close();
                return room;
            }
            reader.Close();
            connection.Close();

            return null;
        }

        public void UpdateRoom(Room room)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction();

            try
            {
                // Update room details
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    UPDATE rooms 
                    SET building_id = :building_id,
                        room_number = :room_number,
                        capacity = :capacity,
                        description = :description,
                        id_file = :id_file,
                        price = :price
                    WHERE room_id = :room_id";

                command.CommandType = CommandType.Text;

                command.Parameters.Add(new OracleParameter("building_id", OracleDbType.Int32) { Value = room.BuildingId });
                command.Parameters.Add(new OracleParameter("room_number", OracleDbType.Varchar2) { Value = room.RoomNumber });
                command.Parameters.Add(new OracleParameter("capacity", OracleDbType.Int32) { Value = room.Capacity });
                command.Parameters.Add(new OracleParameter("description", OracleDbType.Varchar2) { Value = room.Description ?? string.Empty });
                command.Parameters.Add(new OracleParameter("id_file", OracleDbType.Int32) { Value = room.IdFile.HasValue ? (object)room.IdFile.Value : DBNull.Value });
                command.Parameters.Add(new OracleParameter("price", OracleDbType.Decimal) { Value = room.Price });
                command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = room.RoomId });

                command.ExecuteNonQuery();

                // Update room equipment
                if (room.Equipment != null)
                {
                    // Delete current equipment
                    var deleteEquipmentCommand = connection.CreateCommand();
                    deleteEquipmentCommand.Transaction = transaction;
                    deleteEquipmentCommand.CommandText = "DELETE FROM room_equipment WHERE room_id = :room_id";
                    deleteEquipmentCommand.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = room.RoomId });
                    deleteEquipmentCommand.ExecuteNonQuery();

                    // Add new equipment
                    if (room.Equipment.Any())
                    {
                        var addEquipmentCommand = connection.CreateCommand();
                        addEquipmentCommand.Transaction = transaction;
                        addEquipmentCommand.CommandText = "INSERT INTO room_equipment (room_id, equipment_id) VALUES (:room_id, :equipment_id)";

                        var roomIdParam = new OracleParameter("room_id", OracleDbType.Int32);
                        var equipmentIdParam = new OracleParameter("equipment_id", OracleDbType.Int32);

                        addEquipmentCommand.Parameters.Add(roomIdParam);
                        addEquipmentCommand.Parameters.Add(equipmentIdParam);

                        foreach (var equipment in room.Equipment)
                        {
                            roomIdParam.Value = room.RoomId;
                            equipmentIdParam.Value = equipment.EquipmentId;
                            addEquipmentCommand.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        public void DeleteRoom(int roomId)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction();

            try
            {
                // Delete associated equipment
                var deleteEquipmentCommand = connection.CreateCommand();
                deleteEquipmentCommand.Transaction = transaction;
                deleteEquipmentCommand.CommandText = "DELETE FROM room_equipment WHERE room_id = :room_id";
                deleteEquipmentCommand.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
                deleteEquipmentCommand.ExecuteNonQuery();

                // Delete room bookings
                var deleteBookingsCommand = connection.CreateCommand();
                deleteBookingsCommand.Transaction = transaction;
                deleteBookingsCommand.CommandText = "DELETE FROM bookings WHERE room_id = :room_id";
                deleteBookingsCommand.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
                deleteBookingsCommand.ExecuteNonQuery();

                // Delete the room itself
                var deleteRoomCommand = connection.CreateCommand();
                deleteRoomCommand.Transaction = transaction;
                deleteRoomCommand.CommandText = "DELETE FROM rooms WHERE room_id = :room_id";
                deleteRoomCommand.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
                deleteRoomCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        public IEnumerable<Booking> GetRoomReservations(int roomId, DateTime startDate, DateTime endDate)
        {
            var bookings = new List<Booking>();

            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT booking_id, user_id, room_id, booking_date, start_time, end_time, status
                FROM bookings
                WHERE room_id = :roomId 
                AND booking_date BETWEEN TRUNC(:startDate) AND TRUNC(:endDate)
                ORDER BY booking_date, start_time";

            command.CommandType = CommandType.Text;
            command.Parameters.Add(new OracleParameter("roomId", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("startDate", OracleDbType.Date) { Value = startDate });
            command.Parameters.Add(new OracleParameter("endDate", OracleDbType.Date) { Value = endDate });

            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                bookings.Add(new Booking
                {
                    BookingId = Convert.ToInt32(reader["booking_id"]),
                    UserId = Convert.ToInt32(reader["user_id"]),
                    RoomId = Convert.ToInt32(reader["room_id"]),
                    BookingDate = Convert.ToDateTime(reader["booking_date"]),
                    StartTime = Convert.ToDateTime(reader["start_time"]),
                    EndTime = Convert.ToDateTime(reader["end_time"]),
                    Status = reader["status"].ToString()
                });
            }
            reader.Close();
            connection.Close();

            return bookings;
        }

        public void AddRoomEquipment(int roomId, IEnumerable<int> equipmentIds)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();

            foreach (var equipmentId in equipmentIds)
            {
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ROOM_EQUIPMENT (ROOM_ID, EQUIPMENT_ID) VALUES (:roomId, :equipmentId)";
                command.CommandType = CommandType.Text;

                command.Parameters.Add(new OracleParameter("roomId", OracleDbType.Int32) { Value = roomId });
                command.Parameters.Add(new OracleParameter("equipmentId", OracleDbType.Int32) { Value = equipmentId });

                command.ExecuteNonQuery();
            }
            connection.Close();
        }

        public List<int> GetEquipmentByRoomId(int roomId)
        {
            var equipmentIds = new List<int>();
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = "SELECT equipment_id FROM room_equipment WHERE room_id = :room_id";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipmentIds.Add(Convert.ToInt32(reader["equipment_id"]));
            }
            reader.Close();
            connection.Close();
            return equipmentIds;
        }

        public List<Equipment> GetEquipmentDetails(List<int> equipmentIds)
        {
            var equipment = new List<Equipment>();
            if (equipmentIds == null || !equipmentIds.Any())
                return equipment;

            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT equipment_id, equipment_name 
                FROM equipment 
                WHERE equipment_id IN (SELECT COLUMN_VALUE FROM TABLE(:equipment_ids))";

            command.CommandType = CommandType.Text;

            var equipmentArray = equipmentIds.ToArray();
            var equipmentParam = new OracleParameter
            {
                ParameterName = "equipment_ids",
                OracleDbType = OracleDbType.Varchar2,
                CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                Value = equipmentArray,
                Size = equipmentArray.Length
            };
            command.Parameters.Add(equipmentParam);

            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipment.Add(new Equipment
                {
                    EquipmentId = Convert.ToInt32(reader["equipment_id"]),
                    Name = reader["equipment_name"].ToString()
                });
            }
            reader.Close();
            connection.Close();
            return equipment;
        }

        public int? GetRoomIdFile(int roomId)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT id_file FROM rooms WHERE room_id = :room_id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var idFile = reader.IsDBNull(reader.GetOrdinal("id_file")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("id_file"));
                reader.Close();
                connection.Close();
                return idFile;
            }
            reader.Close();
            connection.Close();

            return null;
        }

        public void AddEquipmentToRoom(int roomId, int equipmentId)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                INSERT INTO room_equipment (room_id, equipment_id)
                VALUES (:room_id, :equipment_id)";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("equipment_id", OracleDbType.Int32) { Value = equipmentId });

            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void RemoveEquipmentFromRoom(int roomId, int equipmentId)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                DELETE FROM room_equipment 
                WHERE room_id = :room_id AND equipment_id = :equipment_id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("equipment_id", OracleDbType.Int32) { Value = equipmentId });

            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public decimal GetRoomUtilization(int roomId, DateTime startDate, DateTime endDate)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                BEGIN
                    sp_calculate_room_utilization(:p_room_id, :p_start_date, :p_end_date, :p_utilization);
                END;";

            command.CommandType = CommandType.Text;

            // Input parameters
            command.Parameters.Add(new OracleParameter("p_room_id", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("p_start_date", OracleDbType.Date) { Value = startDate });
            command.Parameters.Add(new OracleParameter("p_end_date", OracleDbType.Date) { Value = endDate });

            // Output parameter
            var utilizationParam = new OracleParameter("p_utilization", OracleDbType.Decimal);
            utilizationParam.Direction = ParameterDirection.Output;
            command.Parameters.Add(utilizationParam);

            connection.Open();
            command.ExecuteNonQuery();

            // Properly convert OracleDecimal to decimal
            var utilization = utilizationParam.Value == DBNull.Value ? 0m : (decimal)((Oracle.ManagedDataAccess.Types.OracleDecimal)utilizationParam.Value).Value;
            connection.Close();
            return utilization;
        }

        public IEnumerable<Room> GetAvailableRooms(RoomFilterParameters filters = null)
        {
            var rooms = new List<Room>();
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            var sql = @"SELECT r.room_id, var.room_number, var.building_name, var.capacity, 
                        var.description, var.price, var.equipment_list
                        FROM V_AVAILABLE_ROOMS var
                        JOIN rooms r ON r.room_number = var.room_number
                        WHERE 1=1";

            var parameters = new List<OracleParameter>();

            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.Name))
                {
                    sql += " AND (LOWER(var.room_number) LIKE LOWER(:name) OR LOWER(var.description) LIKE LOWER(:name))";
                    parameters.Add(new OracleParameter("name", OracleDbType.Varchar2) { Value = $"%{filters.Name}%" });
                }

                if (filters.MinPrice.HasValue)
                {
                    sql += " AND var.price >= :min_price";
                    parameters.Add(new OracleParameter("min_price", OracleDbType.Decimal) { Value = filters.MinPrice.Value });
                }

                if (filters.MaxPrice.HasValue)
                {
                    sql += " AND var.price <= :max_price";
                    parameters.Add(new OracleParameter("max_price", OracleDbType.Decimal) { Value = filters.MaxPrice.Value });
                }

                if (filters.MinCapacity.HasValue)
                {
                    sql += " AND var.capacity >= :min_capacity";
                    parameters.Add(new OracleParameter("min_capacity", OracleDbType.Int32) { Value = filters.MinCapacity.Value });
                }

                if (filters.MaxCapacity.HasValue)
                {
                    sql += " AND var.capacity <= :max_capacity";
                    parameters.Add(new OracleParameter("max_capacity", OracleDbType.Int32) { Value = filters.MaxCapacity.Value });
                }
            }

            command.CommandText = sql;
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var room = new Room
                {
                    RoomId = Convert.ToInt32(reader["room_id"]),
                    RoomNumber = reader["room_number"].ToString(),
                    Capacity = Convert.ToInt32(reader["capacity"]),
                    Description = reader["description"]?.ToString(),
                    Price = Convert.ToDecimal(reader["price"]),
                    Equipment = reader["equipment_list"]?.ToString()?.Split(", ")
                        .Select(e => new Equipment { Name = e })
                        .ToList() ?? new List<Equipment>()
                };
                rooms.Add(room);
            }
            reader.Close();
            connection.Close();

            return rooms;
        }

        public IEnumerable<OptimalRoom> FindOptimalRooms(int capacity, decimal maxPrice, string[] requiredEquipment, DateTime date)
        {
            var rooms = new List<OptimalRoom>();
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                DECLARE
                    v_result SYS_REFCURSOR;
                BEGIN
                    v_result := find_optimal_rooms(:capacity, :max_price, :equipment, :date);
                    :result_cursor := v_result;
                END;";

            command.Parameters.Add(new OracleParameter("capacity", OracleDbType.Int32) { Value = capacity });
            command.Parameters.Add(new OracleParameter("max_price", OracleDbType.Decimal) { Value = maxPrice });
            command.Parameters.Add(new OracleParameter("equipment", OracleDbType.Varchar2) { Value = string.Join(",", requiredEquipment) });
            command.Parameters.Add(new OracleParameter("date", OracleDbType.Date) { Value = date });

            var resultParam = new OracleParameter("result_cursor", OracleDbType.RefCursor);
            resultParam.Direction = ParameterDirection.Output;
            command.Parameters.Add(resultParam);

            connection.Open();
            command.ExecuteNonQuery();

            var reader = ((OracleRefCursor)resultParam.Value).GetDataReader();
            while (reader.Read())
            {
                rooms.Add(new OptimalRoom
                {
                    RoomId = Convert.ToInt32(reader["room_id"]),
                    RoomNumber = reader["room_number"].ToString(),
                    Capacity = Convert.ToInt32(reader["capacity"]),
                    Price = Convert.ToDecimal(reader["price"]),
                    EquipmentMatchCount = Convert.ToInt32(reader["equipment_match"]),
                    TotalScore = Convert.ToDecimal(reader["total_score"])
                });
            }
            reader.Close();
            connection.Close();

            return rooms;
        }

        public IEnumerable<RoomOccupancyData> AnalyzeRoomOccupancy(int roomId, DateTime startDate, int daysAhead)
        {
            var occupancyData = new List<RoomOccupancyData>();
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                DECLARE
                    v_result SYS_REFCURSOR;
                BEGIN
                    v_result := analyze_room_occupancy(:room_id, :start_date, :days_ahead);
                    :result_cursor := v_result;
                END;";

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("start_date", OracleDbType.Date) { Value = startDate });
            command.Parameters.Add(new OracleParameter("days_ahead", OracleDbType.Int32) { Value = daysAhead });

            var resultParam = new OracleParameter("result_cursor", OracleDbType.RefCursor);
            resultParam.Direction = ParameterDirection.Output;
            command.Parameters.Add(resultParam);

            connection.Open();
            command.ExecuteNonQuery();

            var reader = ((OracleRefCursor)resultParam.Value).GetDataReader();
            while (reader.Read())
            {
                occupancyData.Add(new RoomOccupancyData
                {
                    SlotDate = Convert.ToDateTime(reader["slot_date"]),
                    BookingsCount = Convert.ToInt32(reader["bookings_count"]),
                    OccupancyPercentage = Convert.ToDecimal(reader["occupancy_percentage"]),
                    MovingAverageBookings = Convert.ToDecimal(reader["moving_avg_bookings"]),
                    DayType = reader["day_type"].ToString()
                });
            }
            reader.Close();
            connection.Close();

            return occupancyData;
        }

        private Room MapRoomFromReader(OracleDataReader reader)
        {
            var room = new Room
            {
                RoomId = Convert.ToInt32(reader["room_id"]),
                BuildingId = Convert.ToInt32(reader["building_id"]),
                RoomNumber = reader["room_number"].ToString(),
                Capacity = Convert.ToInt32(reader["capacity"]),
                Description = reader["description"].ToString(),
                Price = Convert.ToDecimal(reader["price"]),
                IdFile = reader.IsDBNull(reader.GetOrdinal("id_file")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("id_file"))
            };

            // Get equipment for the room
            room.Equipment = GetEquipmentForRoom(room.RoomId);

            return room;
        }

        private List<Equipment> GetEquipmentForRoom(int roomId)
        {
            var equipment = new List<Equipment>();
            var connection = _dbConnectionFactory.CreateConnection();
            var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT e.equipment_id, vre.equipment_name 
                FROM V_ROOM_EQUIPMENT vre
                JOIN equipment e ON e.equipment_name = vre.equipment_name
                WHERE vre.room_number = (SELECT room_number FROM rooms WHERE room_id = :room_id)";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipment.Add(new Equipment
                {
                    EquipmentId = Convert.ToInt32(reader["equipment_id"]),
                    Name = reader["equipment_name"].ToString()
                });
            }
            reader.Close();
            connection.Close();
            return equipment;
        }
    }
}
