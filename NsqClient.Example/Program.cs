using System;
using System.Globalization;
using System.Threading.Tasks;

namespace NsqClient.Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            INsqConsumer connection = new NsqConsumer(new NsqConsumerOptions()
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
            
            await connection.ConnectAsync();
            
            INsqProducer producer = new NsqProducer(new NsqProducerOptions());
            
            await producer.ConnectAsync();

            Parallel.For(0, 1000,
                async i =>
                {
                    await producer.PublishAsync("test", DateTime.Now.ToString("o"));
                });

            Console.ReadLine();
        }

        private static void OnReconnected(object sender, NsqReconnectionEventArgs e)
        {
            Console.WriteLine($"OnReconnected: Reconnected after {e.Attempts} attempts in {e.ReconnectedAfter.TotalSeconds} seconds");
        }

        private static void OnDisconnected(object sender, NsqDisconnectionEventArgs e)
        {
            Console.WriteLine("OnDisconnected: Disconnected. Will reconnect? " + e.WillReconnect);
        }

        private static async void OnMessage(object sender, NsqMessageEventArgs eventArgs)
        {
            Console.WriteLine(DateTime.Now.ToString("o") + " " + eventArgs.Attempt + " " + eventArgs.BodyString);

            await eventArgs.Finish();
        }

        private static void OnError(object sender, NsqErrorEventArgs eventArgs)
        {
            Console.WriteLine("OnError: {0}", eventArgs.Exception);
        }
    }
}
