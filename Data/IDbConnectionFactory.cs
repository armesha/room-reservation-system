using Oracle.ManagedDataAccess.Client;

namespace RoomReservationSystem.Data
{
    public interface IDbConnectionFactory
    {
        OracleConnection CreateConnection();
    }
}
