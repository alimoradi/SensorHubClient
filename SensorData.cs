using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
namespace SensorHubClient
{
    public class SensorData
    {
        public Int64 timestamp;
        public string id;
        public string type;
        public Dictionary<string, string> data;

        public DateTime Timestamp
        {
            get
            {
                return new DateTime(timestamp);
            }
        }
    }
    public class SensorDataManager
    {
        private static SensorData Parse(string data)
        {
            return JsonConvert.DeserializeObject<SensorData>(data);
        }
        public static async void Insert(string data)
        {
            try
            {
                var dataObject = Parse(data);
                Console.WriteLine("Parsing data successful");
                await Database.Instance().Insert(dataObject);
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing data.");

            }

        }
    }
}