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
            var connection = new NsqConnection("localhost", 4150);
            await connection.Connect();
        }
    }
}
