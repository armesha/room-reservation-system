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
            
            // First, delete the existing record
            command.CommandText = "DELETE FROM user_country WHERE user_id = :user_id";
            command.Parameters.Add(new OracleParameter("user_id", OracleDbType.Int32) { Value = userId });
            command.ExecuteNonQuery();
            
            if (!string.IsNullOrEmpty(countryCode))
            {
                // Then add a new one
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

        public void UpdateCountry(Country country)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE countries 
                SET country_name = :country_name,
                    country_code = :country_code
                WHERE country_id = :country_id";

            command.Parameters.Add(new OracleParameter("country_name", OracleDbType.Varchar2) { Value = country.CountryName });
            command.Parameters.Add(new OracleParameter("country_code", OracleDbType.Varchar2) { Value = country.CountryCode });
            command.Parameters.Add(new OracleParameter("country_id", OracleDbType.Int32) { Value = country.CountryId });

            var rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException($"Country with ID {country.CountryId} not found");
            }
        }

        public void DeleteCountry(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            // First check if the country is referenced in user_country table
            command.CommandText = @"
                SELECT COUNT(*) 
                FROM user_country 
                WHERE country_code IN (
                    SELECT country_code 
                    FROM countries 
                    WHERE country_id = :country_id
                )";
            command.Parameters.Add(new OracleParameter("country_id", OracleDbType.Int32) { Value = id });

            var count = Convert.ToInt32(command.ExecuteScalar());
            if (count > 0)
            {
                throw new InvalidOperationException("Cannot delete country that is assigned to users");
            }

            // If no references exist, delete the country
            command.CommandText = "DELETE FROM countries WHERE country_id = :country_id";
            var rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException($"Country with ID {id} not found");
            }
        }

        public Country AddCountry(Country country)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            // Check if country code or name already exists
            command.CommandText = @"
                SELECT 
                    CASE 
                        WHEN EXISTS (SELECT 1 FROM countries WHERE UPPER(country_code) = UPPER(:country_code)) THEN 'code'
                        WHEN EXISTS (SELECT 1 FROM countries WHERE UPPER(country_name) = UPPER(:country_name)) THEN 'name'
                        ELSE NULL 
                    END as conflict
                FROM dual";

            command.Parameters.Add(new OracleParameter("country_code", OracleDbType.Varchar2) { Value = country.CountryCode.ToUpper() });
            command.Parameters.Add(new OracleParameter("country_name", OracleDbType.Varchar2) { Value = country.CountryName });
            
            using var reader = command.ExecuteReader();
            if (reader.Read() && !reader.IsDBNull(0))
            {
                var conflict = reader.GetString(0);
                if (conflict == "code")
                {
                    throw new InvalidOperationException($"Country with code '{country.CountryCode}' already exists");
                }
                else if (conflict == "name")
                {
                    throw new InvalidOperationException($"Country with name '{country.CountryName}' already exists");
                }
            }

            // Insert new country
            command.CommandText = @"
                INSERT INTO countries (country_id, country_name, country_code) 
                VALUES (seq_countries.NEXTVAL, :country_name, :country_code)
                RETURNING country_id INTO :country_id";

            command.Parameters.Clear();
            command.Parameters.Add(new OracleParameter("country_name", OracleDbType.Varchar2) { Value = country.CountryName });
            command.Parameters.Add(new OracleParameter("country_code", OracleDbType.Varchar2) { Value = country.CountryCode.ToUpper() });
            
            var countryIdParam = new OracleParameter("country_id", OracleDbType.Int32);
            countryIdParam.Direction = System.Data.ParameterDirection.Output;
            command.Parameters.Add(countryIdParam);

            command.ExecuteNonQuery();

            if (countryIdParam.Value == DBNull.Value)
            {
                throw new InvalidOperationException("Failed to get new country ID");
            }

            country.CountryId = Convert.ToInt32(countryIdParam.Value.ToString());
            country.CountryCode = country.CountryCode.ToUpper();
            return country;
        }
    }
}
