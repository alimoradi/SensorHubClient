using System;

namespace SensorHubClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo cki;

            Console.Clear();
            // Add event handler for ControlC in order to gracefully stop
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

            SensorClient.Instance().Run();
            


            while (true)
            {
                Console.WriteLine("Press  'X' or 'Ctrl+C' to quit");


                // Start a console read operation. Do not display the input.
                cki = Console.ReadKey(true);

                // Exit if the user pressed the 'X' key.
                if (cki.Key == ConsoleKey.X) Stop();
            }
        }
        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nCommencing Exit...");

            // Set the Cancel property to true to prevent the process from terminating.
            args.Cancel = true;
            Stop();
        }
        protected static async void Stop()
        {
           Console.WriteLine("Stop Called");
           await SensorClient.Instance().StopAsync();
           Environment.Exit(0);
        }
    }
}
