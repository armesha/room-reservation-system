using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace RoomReservationSystem.Data
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public OracleConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("OracleDb");
            return new OracleConnection(connectionString);
        }
    }
}
