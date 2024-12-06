using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Data;
using RoomReservationSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

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
            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert into rooms table
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO rooms (room_id, building_id, room_number, capacity, description, image, price) 
                    VALUES (seq_rooms.NEXTVAL, :building_id, :room_number, :capacity, :description, :image, :price) 
                    RETURNING room_id INTO :room_id";
                
                command.CommandType = CommandType.Text;

                var roomIdParam = new OracleParameter("room_id", OracleDbType.Int32);
                roomIdParam.Direction = ParameterDirection.Output;

                command.Parameters.Add(new OracleParameter("building_id", OracleDbType.Int32) { Value = room.BuildingId });
                command.Parameters.Add(new OracleParameter("room_number", OracleDbType.Varchar2) { Value = room.RoomNumber });
                command.Parameters.Add(new OracleParameter("capacity", OracleDbType.Int32) { Value = room.Capacity });
                command.Parameters.Add(new OracleParameter("description", OracleDbType.Varchar2) { Value = room.Description ?? string.Empty });
                command.Parameters.Add(new OracleParameter("image", OracleDbType.Blob) { Value = room.Image ?? new byte[0] });
                command.Parameters.Add(new OracleParameter("price", OracleDbType.Decimal) { Value = room.Price });
                command.Parameters.Add(roomIdParam);

                command.ExecuteNonQuery();
                
                int newRoomId = Convert.ToInt32(roomIdParam.Value.ToString());

                // Insert equipment associations
                if (room.Equipment != null && room.Equipment.Any())
                {
                    using var equipmentCommand = connection.CreateCommand();
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
        }

        public IEnumerable<Room> GetAllRooms(int? limit = null, int? offset = null, RoomFilterParameters filters = null)
        {
            var rooms = new List<Room>();
            using var connection = _dbConnectionFactory.CreateConnection();

            // Step 1: Get room IDs with filtering
            using (var command = connection.CreateCommand())
            {
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
                            WHERE equipment_id IN (SELECT COLUMN_VALUE FROM TABLE(:p_equipment_ids))
                            GROUP BY room_id 
                            HAVING COUNT(*) = :p_equipment_count
                        )";

                        var equipmentArray = filters.EquipmentIds.ToArray();
                        var equipmentParam = new OracleParameter
                        {
                            ParameterName = "p_equipment_ids",
                            OracleDbType = OracleDbType.Varchar2,
                            CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                            Value = equipmentArray,
                            Size = equipmentArray.Length
                        };
                        parameters.Add(equipmentParam);
                        parameters.Add(new OracleParameter("p_equipment_count", OracleDbType.Int32) { Value = equipmentArray.Length });
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
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roomIds.Add(Convert.ToInt32(reader["room_id"]));
                    }
                }

                // Step 2: Get full room data for each ID
                foreach (var roomId in roomIds)
                {
                    var room = GetRoomById(roomId);
                    if (room != null)
                    {
                        rooms.Add(room);
                    }
                }
            }

            return rooms;
        }

        public IEnumerable<Room> GetRandomRooms(int count)
        {
            var rooms = new List<Room>();

            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            
            command.CommandText = "SELECT * FROM (SELECT * FROM rooms ORDER BY DBMS_RANDOM.VALUE) WHERE ROWNUM <= :count";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new OracleParameter("count", OracleDbType.Int32) { Value = count });

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rooms.Add(MapRoomFromReader(reader));
            }

            return rooms;
        }

        public IEnumerable<DateTime> GetReservedDates(int roomId, DateTime startDate, DateTime endDate)
        {
            var reservedDates = new List<DateTime>();

            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            
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
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                reservedDates.Add(Convert.ToDateTime(reader["reserved_date"]));
            }

            return reservedDates;
        }

        public Room GetRoomById(int roomId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM rooms WHERE room_id = :room_id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapRoomFromReader(reader);
            }

            return null;
        }

        public void UpdateRoom(Room room)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Update room details
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    UPDATE rooms 
                    SET building_id = :building_id, 
                        room_number = :room_number, 
                        capacity = :capacity, 
                        description = :description, 
                        image = :image, 
                        price = :price 
                    WHERE room_id = :room_id";
                
                command.CommandType = CommandType.Text;

                command.Parameters.Add(new OracleParameter("building_id", OracleDbType.Int32) { Value = room.BuildingId });
                command.Parameters.Add(new OracleParameter("room_number", OracleDbType.Varchar2) { Value = room.RoomNumber });
                command.Parameters.Add(new OracleParameter("capacity", OracleDbType.Int32) { Value = room.Capacity });
                command.Parameters.Add(new OracleParameter("description", OracleDbType.Varchar2) { Value = room.Description ?? string.Empty });
                command.Parameters.Add(new OracleParameter("image", OracleDbType.Blob) { Value = room.Image ?? new byte[0] });
                command.Parameters.Add(new OracleParameter("price", OracleDbType.Decimal) { Value = room.Price });
                command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = room.RoomId });

                command.ExecuteNonQuery();

                // Update equipment associations
                // First, remove all existing equipment associations
                using var deleteCommand = connection.CreateCommand();
                deleteCommand.Transaction = transaction;
                deleteCommand.CommandText = "DELETE FROM room_equipment WHERE room_id = :room_id";
                deleteCommand.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = room.RoomId });
                deleteCommand.ExecuteNonQuery();

                // Then add new equipment associations
                if (room.Equipment != null && room.Equipment.Any())
                {
                    using var equipmentCommand = connection.CreateCommand();
                    equipmentCommand.Transaction = transaction;
                    equipmentCommand.CommandText = "INSERT INTO room_equipment (room_id, equipment_id) VALUES (:room_id, :equipment_id)";
                    
                    var roomIdParam = new OracleParameter("room_id", OracleDbType.Int32);
                    var equipmentIdParam = new OracleParameter("equipment_id", OracleDbType.Int32);
                    
                    equipmentCommand.Parameters.Add(roomIdParam);
                    equipmentCommand.Parameters.Add(equipmentIdParam);

                    foreach (var equipment in room.Equipment)
                    {
                        roomIdParam.Value = room.RoomId;
                        equipmentIdParam.Value = equipment.EquipmentId;
                        equipmentCommand.ExecuteNonQuery();
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

        public void DeleteRoom(int roomId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM rooms WHERE room_id = :room_id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            command.ExecuteNonQuery();
        }

        public IEnumerable<Booking> GetRoomReservations(int roomId, DateTime startDate, DateTime endDate)
        {
            var bookings = new List<Booking>();

            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            
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
            using var reader = command.ExecuteReader();
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

            return bookings;
        }

        public void AddRoomEquipment(int roomId, IEnumerable<int> equipmentIds)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            connection.Open();

            foreach (var equipmentId in equipmentIds)
            {
                using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ROOM_EQUIPMENT (ROOM_ID, EQUIPMENT_ID) VALUES (:roomId, :equipmentId)";
                command.CommandType = CommandType.Text;

                command.Parameters.Add(new OracleParameter("roomId", OracleDbType.Int32) { Value = roomId });
                command.Parameters.Add(new OracleParameter("equipmentId", OracleDbType.Int32) { Value = equipmentId });

                command.ExecuteNonQuery();
            }
        }

        public List<int> GetEquipmentByRoomId(int roomId)
        {
            var equipmentIds = new List<int>();
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            
            command.CommandText = "SELECT equipment_id FROM room_equipment WHERE room_id = :room_id";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipmentIds.Add(Convert.ToInt32(reader["equipment_id"]));
            }
            return equipmentIds;
        }

        public List<Equipment> GetEquipmentDetails(List<int> equipmentIds)
        {
            var equipment = new List<Equipment>();
            if (equipmentIds == null || !equipmentIds.Any())
                return equipment;

            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            
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
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipment.Add(new Equipment
                {
                    EquipmentId = Convert.ToInt32(reader["equipment_id"]),
                    Name = reader["equipment_name"].ToString()
                });
            }
            return equipment;
        }

        public byte[] GetRoomImage(int roomId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT image FROM rooms WHERE room_id = :room_id";
            command.CommandType = CommandType.Text;
            command.InitialLOBFetchSize = -1;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader["image"] as byte[];
            }

            return null;
        }

        public void AddEquipmentToRoom(int roomId, int equipmentId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            
            command.CommandText = @"
                INSERT INTO room_equipment (room_id, equipment_id)
                VALUES (:room_id, :equipment_id)";
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
            
            command.CommandText = @"
                DELETE FROM room_equipment 
                WHERE room_id = :room_id AND equipment_id = :equipment_id";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });
            command.Parameters.Add(new OracleParameter("equipment_id", OracleDbType.Int32) { Value = equipmentId });

            connection.Open();
            command.ExecuteNonQuery();
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
                Price = Convert.ToDecimal(reader["price"])
            };

            if (!reader.IsDBNull(reader.GetOrdinal("image")))
            {
                var blob = reader.GetOracleBlob(reader.GetOrdinal("image"));
                if (blob != null && blob.Length > 0)
                {
                    room.Image = new byte[blob.Length];
                    blob.Read(room.Image, 0, (int)blob.Length);
                }
            }

            // Get equipment for the room
            room.Equipment = GetEquipmentForRoom(room.RoomId);

            return room;
        }

        private List<Equipment> GetEquipmentForRoom(int roomId)
        {
            var equipment = new List<Equipment>();
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = connection.CreateCommand();
            
            command.CommandText = @"
                SELECT e.equipment_id, e.equipment_name 
                FROM equipment e
                JOIN room_equipment re ON e.equipment_id = re.equipment_id
                WHERE re.room_id = :room_id";
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = roomId });

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipment.Add(new Equipment
                {
                    EquipmentId = Convert.ToInt32(reader["equipment_id"]),
                    Name = reader["equipment_name"].ToString()
                });
            }
            return equipment;
        }
    }
}
