using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Data;
using RoomReservationSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoomReservationSystem.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public EventRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Event> GetAllEvents()
        {
            var events = new List<Event>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT event_id, event_name, event_date, description, booking_id, created_by, created_at 
                                  FROM events";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new Event
                {
                    EventId = Convert.ToInt32(reader["event_id"]),
                    EventName = reader["event_name"].ToString(),
                    EventDate = Convert.ToDateTime(reader["event_date"]),
                    Description = reader["description"].ToString(),
                    BookingId = reader["booking_id"] != DBNull.Value ? Convert.ToInt32(reader["booking_id"]) : null,
                    CreatedBy = Convert.ToInt32(reader["created_by"]),
                    CreatedAt = Convert.ToDateTime(reader["created_at"])
                });
            }
            return events;
        }

        public Event GetEventById(int eventId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT event_id, event_name, event_date, description, booking_id, created_by, created_at 
                                  FROM events 
                                  WHERE event_id = :event_id";
            command.Parameters.Add(new OracleParameter("event_id", OracleDbType.Int32) { Value = eventId });

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Event
                {
                    EventId = Convert.ToInt32(reader["event_id"]),
                    EventName = reader["event_name"].ToString(),
                    EventDate = Convert.ToDateTime(reader["event_date"]),
                    Description = reader["description"].ToString(),
                    BookingId = reader["booking_id"] != DBNull.Value ? Convert.ToInt32(reader["booking_id"]) : null,
                    CreatedBy = Convert.ToInt32(reader["created_by"]),
                    CreatedAt = Convert.ToDateTime(reader["created_at"])
                };
            }
            return null;
        }

        public Event GetEventByBookingId(int bookingId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT event_id, event_name, event_date, description, booking_id, created_by, created_at 
                                  FROM events 
                                  WHERE booking_id = :booking_id";
            command.Parameters.Add(new OracleParameter("booking_id", OracleDbType.Int32) { Value = bookingId });

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Event
                {
                    EventId = Convert.ToInt32(reader["event_id"]),
                    EventName = reader["event_name"].ToString(),
                    EventDate = Convert.ToDateTime(reader["event_date"]),
                    Description = reader["description"].ToString(),
                    BookingId = reader["booking_id"] != DBNull.Value ? Convert.ToInt32(reader["booking_id"]) : null,
                    CreatedBy = Convert.ToInt32(reader["created_by"]),
                    CreatedAt = Convert.ToDateTime(reader["created_at"])
                };
            }
            return null;
        }

        public void AddEvent(Event eventEntity)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "sp_add_event";

            // Input parameters
            command.Parameters.Add(new OracleParameter("p_event_name", OracleDbType.Varchar2) { Value = eventEntity.EventName });
            command.Parameters.Add(new OracleParameter("p_event_date", OracleDbType.Date) { Value = eventEntity.EventDate });
            command.Parameters.Add(new OracleParameter("p_description", OracleDbType.Varchar2) { Value = (object)eventEntity.Description ?? DBNull.Value });
            command.Parameters.Add(new OracleParameter("p_booking_id", OracleDbType.Int32) { Value = eventEntity.BookingId.HasValue ? (object)eventEntity.BookingId.Value : DBNull.Value });
            command.Parameters.Add(new OracleParameter("p_created_by", OracleDbType.Int32) { Value = eventEntity.CreatedBy });
            command.Parameters.Add(new OracleParameter("p_parent_event_id", OracleDbType.Int32) { Value = eventEntity.ParentEventId.HasValue ? (object)eventEntity.ParentEventId.Value : DBNull.Value });

            // Output parameter
            var newEventIdParam = new OracleParameter("p_new_event_id", OracleDbType.Int32) { Direction = ParameterDirection.Output };
            command.Parameters.Add(newEventIdParam);

            command.ExecuteNonQuery();
            eventEntity.EventId = Convert.ToInt32(newEventIdParam.Value);
        }

        public void UpdateEvent(Event eventEntity)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE events 
                                  SET event_name = :event_name,
                                      event_date = :event_date,
                                      description = :description,
                                      booking_id = :booking_id,
                                      created_by = :created_by,
                                      created_at = :created_at
                                  WHERE event_id = :event_id";

            command.Parameters.Add(new OracleParameter("event_name", OracleDbType.Varchar2) { Value = eventEntity.EventName });
            command.Parameters.Add(new OracleParameter("event_date", OracleDbType.Date) { Value = eventEntity.EventDate });
            command.Parameters.Add(new OracleParameter("description", OracleDbType.Varchar2) { Value = eventEntity.Description });
            command.Parameters.Add(new OracleParameter("booking_id", OracleDbType.Int32) { 
                Value = eventEntity.BookingId.HasValue ? (object)eventEntity.BookingId.Value : DBNull.Value 
            });
            command.Parameters.Add(new OracleParameter("created_by", OracleDbType.Int32) { Value = eventEntity.CreatedBy });
            command.Parameters.Add(new OracleParameter("created_at", OracleDbType.Date) { Value = eventEntity.CreatedAt });
            command.Parameters.Add(new OracleParameter("event_id", OracleDbType.Int32) { Value = eventEntity.EventId });

            command.ExecuteNonQuery();
        }

        public void DeleteEvent(int eventId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"DELETE FROM events WHERE event_id = :event_id";
            command.Parameters.Add(new OracleParameter("event_id", OracleDbType.Int32) { Value = eventId });

            command.ExecuteNonQuery();
        }

        public IEnumerable<Event> GetEventHierarchy(int? parentId = null)
        {
            var events = new List<Event>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            
            var sql = "SELECT * FROM EventHierarchyView";
            if (parentId.HasValue)
            {
                sql += " WHERE parent_event_id = :parentId";
                var parameter = command.CreateParameter();
                parameter.ParameterName = ":parentId";
                parameter.Value = parentId.Value;
                command.Parameters.Add(parameter);
            }
            sql += " ORDER BY event_date";
            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new Event
                {
                    EventId = Convert.ToInt32(reader["event_id"]),
                    EventName = reader["event_name"].ToString(),
                    EventDate = Convert.ToDateTime(reader["event_date"]),
                    Description = reader["description"].ToString(),
                    BookingId = reader["booking_id"] != DBNull.Value ? Convert.ToInt32(reader["booking_id"]) : null,
                    ParentEventId = reader["parent_event_id"] != DBNull.Value ? Convert.ToInt32(reader["parent_event_id"]) : null,
                    CreatedBy = Convert.ToInt32(reader["created_by"]),
                    CreatedAt = Convert.ToDateTime(reader["created_at"])
                });
            }
            return events;
        }

        public IEnumerable<Event> GetUpcomingEvents()
        {
            var events = new List<Event>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM UpcomingEventsView";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new Event
                {
                    EventId = Convert.ToInt32(reader["event_id"]),
                    EventName = reader["event_name"].ToString(),
                    EventDate = Convert.ToDateTime(reader["event_date"]),
                    Description = reader["description"].ToString(),
                    BookingId = reader["booking_id"] != DBNull.Value ? Convert.ToInt32(reader["booking_id"]) : null,
                    CreatedBy = Convert.ToInt32(reader["created_by"]),
                    EventPath = reader["event_path"].ToString()
                });
            }
            return events;
        }

        public class EventBookingDetail : Event
        {
            public int? RoomId { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string Status { get; set; }
        }

        public IEnumerable<Models.EventBookingDetail> GetEventBookingDetails()
        {
            var events = new List<Models.EventBookingDetail>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM EventBookingDetailsView";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new Models.EventBookingDetail
                {
                    EventId = Convert.ToInt32(reader["event_id"]),
                    EventName = reader["event_name"].ToString(),
                    EventDate = Convert.ToDateTime(reader["event_date"]),
                    BookingId = reader["booking_id"] != DBNull.Value ? Convert.ToInt32(reader["booking_id"]) : null,
                    RoomId = reader["room_id"] != DBNull.Value ? Convert.ToInt32(reader["room_id"]) : null,
                    StartTime = reader["start_time"] != DBNull.Value ? Convert.ToDateTime(reader["start_time"]) : null,
                    EndTime = reader["end_time"] != DBNull.Value ? Convert.ToDateTime(reader["end_time"]) : null,
                    Status = reader["status"].ToString()
                });
            }
            return events;
        }
    }
}
