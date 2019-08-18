using System;
using System.Globalization;
using System.Threading.Tasks;
using NsqClient;

namespace NsqClient.Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connection = new NsqConnection(new NsqConnectionOptions()
            {
                Hostname = "localhost",
                Port = 4150,
                Topic = "test",
                Channel = "test",
                MaxInFlight = 5
            });

            connection.OnError += OnError;
            connection.OnMessage += OnMessage;
            connection.OnDisconnected += OnDisconnected;
            connection.OnReconnected += OnReconnected;
            
            await connection.Connect();

            await connection.Publish("test", DateTime.Now.ToString(CultureInfo.CurrentCulture));

            Console.ReadLine();
        }

        private static void OnReconnected(object sender, NsqReconnectionEventArgs e)
        {
            Console.WriteLine($"Reconnected after {e.Attempts} attempts in {e.ReconnectedAfter.TotalSeconds} seconds");
        }

        private static void OnDisconnected(object sender, NsqDisconnectionEventArgs e)
        {
            Console.WriteLine("Disconnected. Will reconnect? " + e.WillReconnect);
        }

        private static async void OnMessage(object sender, NsqMessageEventArgs eventArgs)
        {
            Console.WriteLine(DateTime.Now.ToString("o") + " " + eventArgs.Attempt + " " + eventArgs.BodyString);

            await eventArgs.Finish();
        }

        private static void OnError(object sender, NsqErrorEventArgs eventArgs)
        {
            Console.WriteLine(eventArgs.Exception);
        }
    }
}
