using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;

namespace SensorHubClient
{
    public class SensorClient
    {
        private static SensorClient _instance;
        HubConnection connection;
        protected SensorClient()
        {

        }
        public static SensorClient Instance()
        {
            if (_instance == null)
            {
                _instance = new SensorClient();
            }

            return _instance;
        }
        public void Run()
        {
            connection = new HubConnectionBuilder()
                                .WithUrl("http://localhost:5020/testhub")
                                .WithAutomaticReconnect()
                                .Build();
            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            connection.Reconnected += connectionId =>
            {
                Console.WriteLine("Client Reconnected.");
                return Task.CompletedTask;
            };
            connection.Reconnecting += error =>
            {
                Console.WriteLine("Client Disconnected, Retrying...");
                return Task.CompletedTask;
            };
            Connect();
        }
        private async void Connect()
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine(message);
                SensorDataManager.Insert(message);
                

            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("Client Connected.");

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
        public async Task<bool> StopAsync()
        {
            Console.WriteLine("Stopping Hub Connection");
            if(connection.State == HubConnectionState.Connected)
            {
                await connection.StopAsync();
                
            }
            return true;
            //connection.Remove
            //stop hub client
        }
    }

}