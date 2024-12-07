using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Data;
using RoomReservationSystem.Models;
using System;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public CountryRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Country> GetAllCountries()
        {
            var countries = new List<Country>();
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT country_id, country_name, country_code 
                FROM countries
                ORDER BY country_name";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                countries.Add(new Country
                {
                    CountryId = Convert.ToInt32(reader["country_id"]),
                    CountryName = reader["country_name"].ToString(),
                    CountryCode = reader["country_code"].ToString()
                });
            }
            return countries;
        }

        public Country GetCountryByCode(string code)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT country_id, country_name, country_code 
                FROM countries 
                WHERE country_code = :country_code";

            command.Parameters.Add(new OracleParameter("country_code", OracleDbType.Varchar2) { Value = code });

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Country
                {
                    CountryId = Convert.ToInt32(reader["country_id"]),
                    CountryName = reader["country_name"].ToString(),
                    CountryCode = reader["country_code"].ToString()
                };
            }
            return null;
        }

        public void SetUserCountry(int userId, string countryCode)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            
            // Сначала удаляем существующую запись
            command.CommandText = "DELETE FROM user_country WHERE user_id = :user_id";
            command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = userId });
            command.ExecuteNonQuery();
            
            if (!string.IsNullOrEmpty(countryCode))
            {
                // Затем добавляем новую
                command.CommandText = @"
                    INSERT INTO user_country (user_id, country_code) 
                    VALUES (:user_id, :country_code)";
                command.Parameters.Clear();
                command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = userId });
                command.Parameters.Add(new OracleParameter("country_code", OracleDbType.Varchar2) { Value = countryCode });
                command.ExecuteNonQuery();
            }
        }

        public string GetUserCountryCode(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT country_code 
                FROM user_country 
                WHERE user_id = :user_id";

            command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = userId });

            var result = command.ExecuteScalar();
            return result != null ? result.ToString() : null;
        }
    }
}
