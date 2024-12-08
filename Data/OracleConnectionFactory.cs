using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace RoomReservationSystem.Data
{
    public class OracleConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public OracleConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
        }

        public OracleConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }
    }
}
