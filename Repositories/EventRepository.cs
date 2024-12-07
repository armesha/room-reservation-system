// Repositories/EventRepository.cs
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
            command.CommandText = @"INSERT INTO events (event_id, event_name, event_date, description, booking_id, created_by, created_at)
                                  VALUES (seq_events.NEXTVAL, :event_name, :event_date, :description, :booking_id, :created_by, :created_at)
                                  RETURNING event_id INTO :event_id";

            command.Parameters.Add(new OracleParameter("event_name", OracleDbType.Varchar2) { Value = eventEntity.EventName });
            command.Parameters.Add(new OracleParameter("event_date", OracleDbType.Date) { Value = eventEntity.EventDate });
            command.Parameters.Add(new OracleParameter("description", OracleDbType.Varchar2) { Value = eventEntity.Description });
            command.Parameters.Add(new OracleParameter("booking_id", OracleDbType.Int32) { 
                Value = eventEntity.BookingId.HasValue ? (object)eventEntity.BookingId.Value : DBNull.Value 
            });
            command.Parameters.Add(new OracleParameter("created_by", OracleDbType.Int32) { Value = eventEntity.CreatedBy });
            command.Parameters.Add(new OracleParameter("created_at", OracleDbType.Date) { Value = eventEntity.CreatedAt });

            var eventIdParam = new OracleParameter("event_id", OracleDbType.Int32);
            eventIdParam.Direction = ParameterDirection.Output;
            command.Parameters.Add(eventIdParam);

            command.ExecuteNonQuery();
            
            var eventIdOracleDecimal = (Oracle.ManagedDataAccess.Types.OracleDecimal)eventIdParam.Value;
            eventEntity.EventId = eventIdOracleDecimal.ToInt32();
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
    }
}
