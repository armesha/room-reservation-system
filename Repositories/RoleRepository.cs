using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Data;
using RoomReservationSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoomReservationSystem.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RoleRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Role GetRoleById(int roleId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT role_id, role_name FROM roles WHERE role_id = :role_id";
            command.Parameters.Add(new OracleParameter("role_id", OracleDbType.Int32) { Value = roleId });

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Role
                {
                    RoleId = Convert.ToInt32(reader["role_id"]),
                    RoleName = reader["role_name"].ToString()
                };
            }
            return null;
        }

        public Role GetRoleByName(string roleName)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT role_id, role_name FROM roles WHERE role_name = :role_name";
            command.Parameters.Add(new OracleParameter("role_name", OracleDbType.Varchar2) { Value = roleName });

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Role
                {
                    RoleId = Convert.ToInt32(reader["role_id"]),
                    RoleName = reader["role_name"].ToString()
                };
            }
            return null;
        }

        public IEnumerable<Role> GetAllRoles()
        {
            var roles = new List<Role>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT role_id, role_name FROM roles";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                roles.Add(new Role
                {
                    RoleId = Convert.ToInt32(reader["role_id"]),
                    RoleName = reader["role_name"].ToString()
                });
            }
            return roles;
        }
    }
}
