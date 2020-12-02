using Npgsql;
using System;
using System.Threading.Tasks;

namespace SensorHubClient
{
    public class Database
    {
        private static Database _instance;
        const string connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=miasidney;";
        const string dbName = "sensor_client_hub";
        NpgsqlConnection serverConnection;
        NpgsqlConnection databaseConnection;
        protected Database()
        {
            serverConnection = new NpgsqlConnection(connectionString);
            databaseConnection = new NpgsqlConnection($"{connectionString}Database={dbName};"); ;
        }
        public static Database Instance()
        {
            if (_instance == null)
            {
                _instance = new Database();
            }

            return _instance;
        }
        public async Task<bool> CreateDB()
        {
            var command = new NpgsqlCommand($"CREATE DATABASE  \"{dbName}\" " +
                                               "WITH OWNER = \"postgres\" " +
                                               "ENCODING = 'UTF8' " +
                                               "CONNECTION LIMIT = -1;", serverConnection);
            try
            {
                await serverConnection.OpenAsync();

                if (!await DBExists())
                {
                    await command.ExecuteNonQueryAsync();
                }
                await serverConnection.CloseAsync();
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Problem creating database");
                return false;
            }

        }
        private async Task<bool> DBExists()
        {
            var command = new NpgsqlCommand($"SELECT COUNT(*) FROM pg_catalog.pg_database WHERE datname = '{dbName}'", serverConnection);
            var reader = await command.ExecuteScalarAsync();
            Console.WriteLine(reader.ToString());
            return int.Parse(reader.ToString()) > 0;
        }
        private async Task<bool> CreateTable(string tableName)
        {
            string[] sensorColumnArray = new string[150];
            for (int i = 0; i < sensorColumnArray.Length; i++)
            {
                sensorColumnArray[i] = $"sensor{i + 1} VARCHAR(25)";
            }
            var command = new NpgsqlCommand($"CREATE TABLE IF NOT EXISTS  \"{tableName}\" ( timestamp timestamp NOT NULL DEFAULT NOW(), id UUID PRIMARY KEY, {String.Join(',', sensorColumnArray)})", databaseConnection);
            await databaseConnection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            databaseConnection.Close();
            return true;
        }
        public async Task<bool> Insert(SensorData data)
        {
            if (await CreateDB())
            {
                try
                {
                    await CreateTable(data.type);
                    var sensorColumns = String.Join(',', data.data.Keys);
                    var sensorValues = String.Join(',', data.data.Values);
                    sensorValues = "";
                    foreach (var item in data.data.Values)
                    {
                        sensorValues += $"'{item}',";
                    }
                    sensorValues = sensorValues.Substring(0, sensorValues.Length - 1);
                    var command = new NpgsqlCommand($"INSERT INTO  \"{data.type}\" (timestamp, id, {sensorColumns} ) VALUES ('{data.Timestamp}', '{data.id}', {sensorValues})", databaseConnection);
                    await databaseConnection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    await databaseConnection.CloseAsync();
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("Problem Creating/Inserting into table.");
                    return false;
                }
            }
            else
                return false;



        }
    }
}