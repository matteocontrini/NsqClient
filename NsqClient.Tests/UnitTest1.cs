using System;
using System.Threading.Tasks;
using Xunit;

namespace NsqClient.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var consumer = new NsqConsumer(new NsqConsumerOptions()
            {
                Hostname = "localhost",
                Port = 4150,
                Topic = "test",
                Channel = "test"
            });

            await consumer.ConnectAsync();
            
            consumer.Dispose();
        }
    }
}
