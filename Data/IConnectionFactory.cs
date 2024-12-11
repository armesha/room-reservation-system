using Oracle.ManagedDataAccess.Client;

namespace RoomReservationSystem.Data
{
    public interface IConnectionFactory
    {
        OracleConnection CreateConnection();
    }
}
