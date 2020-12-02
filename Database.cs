using Npgsql;
using System;
using System.Threading.Tasks;
using System.Data;
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
        public void CreateDB()
        {
            var command = new NpgsqlCommand($"CREATE DATABASE  \"{dbName}\" " +
                                               "WITH OWNER = \"postgres\" " +
                                               "ENCODING = 'UTF8' " +
                                               "CONNECTION LIMIT = -1;", serverConnection);
            try
            {
                OpenServerConnection();
                if (!DBExists())
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem creating database");
                Console.WriteLine(ex.Message);
            }

        }
        private bool DBExists()
        {
            var command = new NpgsqlCommand($"SELECT COUNT(*) FROM pg_catalog.pg_database WHERE datname = '{dbName}'", serverConnection);
            var reader = command.ExecuteScalar();
            return int.Parse(reader.ToString()) > 0;
        }
        private void CreateTable(string tableName)
        {
            string[] sensorColumnArray = new string[150];
            for (int i = 0; i < sensorColumnArray.Length; i++)
            {
                sensorColumnArray[i] = $"sensor{i + 1} VARCHAR(25)";
            }
            var command = new NpgsqlCommand($"CREATE TABLE IF NOT EXISTS  \"{tableName}\" ( timestamp bigint, id UUID PRIMARY KEY, {String.Join(',', sensorColumnArray)})", databaseConnection);

            OpenDBConnection();
            command.ExecuteNonQuery();


        }
        private void OpenDBConnection()
        {
            if (databaseConnection.State == ConnectionState.Closed
            || databaseConnection.State == ConnectionState.Broken)
            {
                databaseConnection.Open();

            }

        }
        private void OpenServerConnection()
        {
            if (serverConnection.State == ConnectionState.Closed
            || serverConnection.State == ConnectionState.Broken)
            {
                serverConnection.Open();

            }

        }
        public void Insert(SensorData data)
        {
            CreateDB();
            
                try
                {
                    CreateTable(data.type);
                    var sensorColumns = String.Join(',', data.data.Keys);
                    var sensorValues = String.Join(',', data.data.Values);
                    sensorValues = "";
                    foreach (var item in data.data.Values)
                    {
                        sensorValues += $"'{item}',";
                    }
                    sensorValues = sensorValues.Substring(0, sensorValues.Length - 1);
                    var command = new NpgsqlCommand($"INSERT INTO  \"{data.type}\" (timestamp, id, {sensorColumns} ) VALUES ({data.timestamp}, '{data.id}', {sensorValues})", databaseConnection);
                    OpenDBConnection();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Problem Creating/Inserting into table.");
                    Console.WriteLine(ex.Message);
                }
          



        }
        public void Stop()
        {
            if (serverConnection.State == ConnectionState.Open)
            {
            Console.WriteLine("Closing DB server connection...");
              serverConnection.Close();
            }
            if (databaseConnection.State == ConnectionState.Open)
            {
            Console.WriteLine("Closing DB connection...");
              databaseConnection.Close();
            }
        }
    }
}