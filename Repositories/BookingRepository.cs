using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Data;
using RoomReservationSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Text.Json;

namespace RoomReservationSystem.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public BookingRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public (IEnumerable<Booking> Bookings, int TotalCount) GetAllBookings(int? limit = null, int? offset = null, BookingFilterParameters filters = null)
        {
            var bookings = new List<Booking>();
            int totalCount = 0;
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            // First get total count
            using var countCommand = connection.CreateCommand();
            var countSql = @"SELECT COUNT(*) 
                            FROM bookings b
                            JOIN users u ON b.user_id = u.user_id 
                            JOIN rooms r ON b.room_id = r.room_id
                            WHERE b.end_time > SYSDATE";

            var parameters = new List<OracleParameter>();
            if (filters != null)
            {
                if (filters.UserId.HasValue)
                {
                    countSql += " AND b.user_id = :user_id";
                    parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = filters.UserId.Value });
                }

                if (filters.RoomId.HasValue)
                {
                    countSql += " AND b.room_id = :room_id";
                    parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = filters.RoomId.Value });
                }

                if (filters.StartDate.HasValue)
                {
                    countSql += " AND b.booking_date >= :start_date";
                    parameters.Add(new OracleParameter("start_date", OracleDbType.Date) { Value = filters.StartDate.Value });
                }

                if (filters.EndDate.HasValue)
                {
                    countSql += " AND b.booking_date <= :end_date";
                    parameters.Add(new OracleParameter("end_date", OracleDbType.Date) { Value = filters.EndDate.Value });
                }

                if (!string.IsNullOrEmpty(filters.Status))
                {
                    countSql += " AND b.status = :status";
                    parameters.Add(new OracleParameter("status", OracleDbType.Varchar2) { Value = filters.Status });
                }

                if (filters.HasEvent.HasValue)
                {
                    countSql += " AND b.has_event = :has_event";
                    parameters.Add(new OracleParameter("has_event", OracleDbType.Int32) { Value = filters.HasEvent.Value ? 1 : 0 });
                }
            }

            countCommand.CommandText = countSql;
            foreach (var parameter in parameters)
            {
                countCommand.Parameters.Add(parameter);
            }
            totalCount = Convert.ToInt32(countCommand.ExecuteScalar());

            // Then get paginated data
            using var command = connection.CreateCommand();
            var sql = @"SELECT b.booking_id, b.user_id, b.room_id, b.booking_date, b.start_time, b.end_time, b.status, b.has_event,
                        u.username, r.room_number, r.id_file
                       FROM bookings b
                       JOIN users u ON b.user_id = u.user_id 
                       JOIN rooms r ON b.room_id = r.room_id
                       WHERE b.end_time > SYSDATE";

            if (filters != null)
            {
                if (filters.UserId.HasValue)
                {
                    sql += " AND b.user_id = :user_id";
                }

                if (filters.RoomId.HasValue)
                {
                    sql += " AND b.room_id = :room_id";
                }

                if (filters.StartDate.HasValue)
                {
                    sql += " AND b.booking_date >= :start_date";
                }

                if (filters.EndDate.HasValue)
                {
                    sql += " AND b.booking_date <= :end_date";
                }

                if (!string.IsNullOrEmpty(filters.Status))
                {
                    sql += " AND b.status = :status";
                }

                if (filters.HasEvent.HasValue)
                {
                    sql += " AND b.has_event = :has_event";
                }
            }

            sql += " ORDER BY b.start_time ASC";

            if (offset.HasValue)
            {
                sql += " OFFSET :offset ROWS";
                parameters.Add(new OracleParameter("offset", OracleDbType.Int32) { Value = offset.Value });
            }
            if (limit.HasValue)
            {
                sql += " FETCH NEXT :limit ROWS ONLY";
                parameters.Add(new OracleParameter("limit", OracleDbType.Int32) { Value = limit.Value });
            }

            command.CommandText = sql;
            foreach (var parameter in parameters)
            {
                var newParam = new OracleParameter(parameter.ParameterName, parameter.OracleDbType)
                {
                    Value = parameter.Value
                };
                command.Parameters.Add(newParam);
            }

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
                    Status = reader["status"].ToString(),
                    HasEvent = Convert.ToBoolean(reader["has_event"]),
                    Username = reader["username"].ToString(),
                    RoomNumber = reader["room_number"].ToString(),
                    RoomFileId = reader["id_file"] != DBNull.Value ? Convert.ToInt32(reader["id_file"]) : null
                });
            }
            return (bookings, totalCount);
        }

        public (IEnumerable<Booking> Bookings, int TotalCount) GetBookingsByUserId(int userId, int? limit = null, int? offset = null, BookingFilterParameters filters = null)
        {
            if (filters == null)
            {
                filters = new BookingFilterParameters();
            }
            filters.UserId = userId;
            return GetAllBookings(limit, offset, filters);
        }

        public Booking GetBookingById(int bookingId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT booking_id, user_id, room_id, booking_date, start_time, end_time, status, has_event 
                                    FROM bookings 
                                    WHERE booking_id = :booking_id";
            command.Parameters.Add(new OracleParameter("booking_id", OracleDbType.Int32) { Value = bookingId });

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Booking
                {
                    BookingId = Convert.ToInt32(reader["booking_id"]),
                    UserId = Convert.ToInt32(reader["user_id"]),
                    RoomId = Convert.ToInt32(reader["room_id"]),
                    BookingDate = Convert.ToDateTime(reader["booking_date"]),
                    StartTime = Convert.ToDateTime(reader["start_time"]),
                    EndTime = Convert.ToDateTime(reader["end_time"]),
                    Status = reader["status"].ToString(),
                    HasEvent = Convert.ToBoolean(reader["has_event"])
                };
            }
            return null;
        }

        public void AddBooking(Booking booking)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            
            command.Transaction = transaction;
            command.CommandText = "BEGIN sp_add_booking(:user_id, :room_id, :booking_date, :start_time, :end_time, " +
                                ":status, :has_event, :event_name, :event_description, :event_created_by, :new_booking_id); END;";
            command.CommandType = CommandType.Text;

            // Input parameters
            command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = booking.UserId });
            command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = booking.RoomId });
            command.Parameters.Add(new OracleParameter("booking_date", OracleDbType.Date) { Value = booking.BookingDate });
            command.Parameters.Add(new OracleParameter("start_time", OracleDbType.TimeStamp) { Value = booking.StartTime });
            command.Parameters.Add(new OracleParameter("end_time", OracleDbType.TimeStamp) { Value = booking.EndTime });
            command.Parameters.Add(new OracleParameter("status", OracleDbType.Varchar2) { Value = booking.Status });
            command.Parameters.Add(new OracleParameter("has_event", OracleDbType.Int32) { Value = booking.HasEvent ? 1 : 0 });

            // Event parameters (can be null)
            if (booking.HasEvent && booking.Event != null)
            {
                command.Parameters.Add(new OracleParameter("event_name", OracleDbType.Varchar2) { Value = booking.Event.EventName });
                command.Parameters.Add(new OracleParameter("event_description", OracleDbType.Varchar2) { Value = booking.Event.Description ?? string.Empty });
                command.Parameters.Add(new OracleParameter("event_created_by", OracleDbType.Int32) { Value = booking.Event.CreatedBy });
            }
            else
            {
                command.Parameters.Add(new OracleParameter("event_name", OracleDbType.Varchar2) { Value = DBNull.Value });
                command.Parameters.Add(new OracleParameter("event_description", OracleDbType.Varchar2) { Value = DBNull.Value });
                command.Parameters.Add(new OracleParameter("event_created_by", OracleDbType.Int32) { Value = DBNull.Value });
            }

            // Output parameter
            var newBookingIdParam = new OracleParameter("new_booking_id", OracleDbType.Int32);
            newBookingIdParam.Direction = ParameterDirection.Output;
            command.Parameters.Add(newBookingIdParam);

            try
            {
                command.ExecuteNonQuery();
                booking.BookingId = Convert.ToInt32(newBookingIdParam.Value.ToString());
                transaction.Commit();
            }
            catch (OracleException ex)
            {
                transaction.Rollback();
                switch (ex.Number)
                {
                    case -20001:
                        throw new InvalidOperationException("The room is already booked for this time period.", ex);
                    case -20002:
                        throw new InvalidOperationException("End time must be greater than start time.", ex);
                    case -20003:
                        throw new InvalidOperationException("Room or user not found.", ex);
                    default:
                        throw;
                }
            }
        }

        public void UpdateBooking(Booking booking)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    UPDATE bookings 
                    SET user_id = :user_id,
                        room_id = :room_id,
                        booking_date = :booking_date,
                        start_time = :start_time,
                        end_time = :end_time,
                        status = :status,
                        has_event = :has_event
                    WHERE booking_id = :booking_id";

                command.Parameters.Add(new OracleParameter("booking_id", OracleDbType.Int32) { Value = booking.BookingId });
                command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = booking.UserId });
                command.Parameters.Add(new OracleParameter("room_id", OracleDbType.Int32) { Value = booking.RoomId });
                command.Parameters.Add(new OracleParameter("booking_date", OracleDbType.Date) { Value = booking.BookingDate });
                command.Parameters.Add(new OracleParameter("start_time", OracleDbType.TimeStamp) { Value = booking.StartTime });
                command.Parameters.Add(new OracleParameter("end_time", OracleDbType.TimeStamp) { Value = booking.EndTime });
                command.Parameters.Add(new OracleParameter("status", OracleDbType.Varchar2) { Value = booking.Status });
                command.Parameters.Add(new OracleParameter("has_event", OracleDbType.Int32) { Value = booking.HasEvent ? 1 : 0 });

                command.ExecuteNonQuery();

                // Update or delete associated event
                if (booking.HasEvent && booking.Event != null)
                {
                    using var eventCommand = connection.CreateCommand();
                    eventCommand.Transaction = transaction;
                    eventCommand.CommandText = @"
                        MERGE INTO events e
                        USING (SELECT :booking_id as booking_id FROM dual) b
                        ON (e.booking_id = b.booking_id)
                        WHEN MATCHED THEN
                            UPDATE SET 
                                event_name = :event_name,
                                event_date = :event_date,
                                description = :description,
                                created_by = :created_by,
                                created_at = :created_at
                        WHEN NOT MATCHED THEN
                            INSERT (event_id, booking_id, event_name, event_date, description, created_by, created_at)
                            VALUES (seq_events.NEXTVAL, :booking_id, :event_name, :event_date, :description, :created_by, :created_at)";

                    eventCommand.Parameters.Add(new OracleParameter("booking_id", OracleDbType.Int32) { Value = booking.BookingId });
                    eventCommand.Parameters.Add(new OracleParameter("event_name", OracleDbType.Varchar2) { Value = booking.Event.EventName });
                    eventCommand.Parameters.Add(new OracleParameter("event_date", OracleDbType.Date) { Value = booking.Event.EventDate });
                    eventCommand.Parameters.Add(new OracleParameter("description", OracleDbType.Varchar2) { Value = booking.Event.Description ?? string.Empty });
                    eventCommand.Parameters.Add(new OracleParameter("created_by", OracleDbType.Int32) { Value = booking.Event.CreatedBy });
                    eventCommand.Parameters.Add(new OracleParameter("created_at", OracleDbType.TimeStamp) { Value = booking.Event.CreatedAt });

                    eventCommand.ExecuteNonQuery();
                }
                else
                {
                    // If event is no longer needed, delete it
                    using var deleteEventCommand = connection.CreateCommand();
                    deleteEventCommand.Transaction = transaction;
                    deleteEventCommand.CommandText = "DELETE FROM events WHERE booking_id = :booking_id";
                    deleteEventCommand.Parameters.Add(new OracleParameter("booking_id", OracleDbType.Int32) { Value = booking.BookingId });
                    deleteEventCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void DeleteBooking(int bookingId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // First delete associated event, if any
                using var deleteEventCommand = connection.CreateCommand();
                deleteEventCommand.Transaction = transaction;
                deleteEventCommand.CommandText = "DELETE FROM events WHERE booking_id = :booking_id";
                deleteEventCommand.Parameters.Add(new OracleParameter("booking_id", OracleDbType.Int32) { Value = bookingId });
                deleteEventCommand.ExecuteNonQuery();

                // Then delete the booking itself
                using var deleteBookingCommand = connection.CreateCommand();
                deleteBookingCommand.Transaction = transaction;
                deleteBookingCommand.CommandText = "DELETE FROM bookings WHERE booking_id = :booking_id";
                deleteBookingCommand.Parameters.Add(new OracleParameter("booking_id", OracleDbType.Int32) { Value = bookingId });
                deleteBookingCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IEnumerable<Invoice> GetUnpaidInvoices()
        {
            var invoices = new List<Invoice>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT i.invoice_id, i.invoice_number, i.booking_id, vbi.total_cost as amount, 
                    CASE WHEN vbi.payment_status = 'Paid' THEN 1 ELSE 0 END as is_paid,
                    vbi.user_id, vbi.room_number, vbi.start_time, vbi.end_time,
                    vbi.username, vbi.room_number, b.building_name
                    FROM invoices i
                    JOIN V_BOOKING_INVOICES vbi ON i.invoice_number = vbi.invoice_number
                    JOIN rooms r ON vbi.room_number = r.room_number
                    JOIN buildings b ON r.building_id = b.building_id
                    WHERE vbi.payment_status = 'Unpaid'";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                invoices.Add(new Invoice
                {
                    InvoiceId = Convert.ToInt32(reader["invoice_id"]),
                    InvoiceNumber = reader["invoice_number"].ToString(),
                    BookingId = Convert.ToInt32(reader["booking_id"]),
                    Amount = Convert.ToDecimal(reader["amount"]),
                    isPaid = Convert.ToInt32(reader["is_paid"]),
                    UserId = Convert.ToInt32(reader["user_id"]),
                    StartTime = Convert.ToDateTime(reader["start_time"]),
                    EndTime = Convert.ToDateTime(reader["end_time"]),
                    Username = reader["username"].ToString(),
                    RoomNumber = reader["room_number"].ToString(),
                    BuildingName = reader["building_name"].ToString()
                });
            }
            return invoices;
        }

        public IEnumerable<Invoice> GetUserInvoices(int userId)
        {
            var invoices = new List<Invoice>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT 
                                i.invoice_id,
                                i.invoice_number,
                                i.booking_id,
                                i.amount,
                                i.invoice_date,
                                i.invoice_date as created_at,
                                i.is_paid,
                                u.username,
                                r.room_number,
                                b.building_name,
                                i.user_id
                            FROM invoices i
                            JOIN bookings bk ON i.booking_id = bk.booking_id
                            JOIN users u ON bk.user_id = u.user_id
                            JOIN rooms r ON bk.room_id = r.room_id
                            JOIN buildings b ON r.building_id = b.building_id
                            WHERE i.user_id = :user_id";
            command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = userId });

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                invoices.Add(new Invoice
                {
                    InvoiceId = Convert.ToInt32(reader["invoice_id"]),
                    InvoiceNumber = reader["invoice_number"].ToString(),
                    BookingId = Convert.ToInt32(reader["booking_id"]),
                    Amount = Convert.ToDecimal(reader["amount"]),
                    isPaid = Convert.ToInt32(reader["is_paid"]),
                    UserId = Convert.ToInt32(reader["user_id"]),
                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                    DueDate = Convert.ToDateTime(reader["invoice_date"]),
                    Username = reader["username"].ToString(),
                    RoomNumber = reader["room_number"].ToString(),
                    BuildingName = reader["building_name"].ToString()
                });
            }
            return invoices;
        }

        public IEnumerable<Invoice> GetPaidInvoices()
        {
            var invoices = new List<Invoice>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT i.invoice_id, i.invoice_number, i.booking_id, i.amount, i.is_paid, b.user_id,
                            b.room_id, b.start_time, b.end_time,
                            u.username, r.room_number, bd.building_name
                            FROM invoices i
                            JOIN bookings b ON i.booking_id = b.booking_id
                            JOIN users u ON b.user_id = u.user_id
                            JOIN rooms r ON b.room_id = r.room_id
                            JOIN buildings bd ON r.building_id = bd.building_id
                            WHERE i.is_paid = 1";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                invoices.Add(new Invoice
                {
                    InvoiceId = Convert.ToInt32(reader["invoice_id"]),
                    InvoiceNumber = reader["invoice_number"].ToString(),
                    BookingId = Convert.ToInt32(reader["booking_id"]),
                    Amount = Convert.ToDecimal(reader["amount"]),
                    isPaid = Convert.ToInt32(reader["is_paid"]),
                    UserId = Convert.ToInt32(reader["user_id"]),
                    RoomId = Convert.ToInt32(reader["room_id"]),
                    StartTime = Convert.ToDateTime(reader["start_time"]),
                    EndTime = Convert.ToDateTime(reader["end_time"]),
                    Username = reader["username"].ToString(),
                    RoomNumber = reader["room_number"].ToString(),
                    BuildingName = reader["building_name"].ToString()
                });
            }
            return invoices;
        }

        public bool CancelInvoices(int[] invoiceIds)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            
            // Convert array to comma-separated string for IN clause
            var invoiceIdList = string.Join(",", invoiceIds);
            
            command.CommandText = @"UPDATE invoices 
                                   SET is_paid = 2
                                   WHERE invoice_id IN (" + invoiceIdList + ")";

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool MarkInvoiceAsPaid(int invoiceId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"UPDATE invoices 
                                   SET is_paid = 1 
                                   WHERE invoice_id = :invoice_id";
            command.Parameters.Add(new OracleParameter("invoice_id", OracleDbType.Int32) { Value = invoiceId });

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool DeleteInvoices(int[] invoiceIds)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            
            // Convert array to comma-separated string for IN clause
            var invoiceIdList = string.Join(",", invoiceIds);
            
            command.CommandText = @"DELETE FROM invoices 
                                   WHERE invoice_id IN (" + invoiceIdList + ")";

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<DailyBookingSummary>> GetDailyBookingSummaryAsync()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT daily_booking_summary() FROM DUAL";
                    var jsonResult = (string)await command.ExecuteScalarAsync();
                    
                    if (string.IsNullOrEmpty(jsonResult))
                        return new List<DailyBookingSummary>();

                    return JsonSerializer.Deserialize<List<DailyBookingSummary>>(
                        jsonResult,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }
            }
        }
    }
}
