using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NsqClient.Commands
{
    class IdentifyCommand : IToBytes
    {
        private readonly byte[] prefix = Encoding.ASCII.GetBytes("IDENTIFY\n");
        private readonly IdentifyRequest identify;
        
        private static readonly JsonSerializerSettings Settings;

        static IdentifyCommand()
        {
            Settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public IdentifyCommand()
        {
            this.identify = new IdentifyRequest()
            {
                Hostname = Environment.MachineName,
                ClientId = Environment.MachineName,
                FeatureNegotiation = true,
                UserAgent = "NsqClient/" + VersionHelper.Version
            };
        }

        public byte[] ToBytes()
        {
            byte[] payload  = SerializeToBytes();
            byte[] payloadLength = BitConverter.GetBytes(payload.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(payloadLength);
            }

            return this.prefix
                .Concat(payloadLength)
                .Concat(payload)
                .ToArray();
        }
        
        private byte[] SerializeToBytes()
        {
            string json = JsonConvert.SerializeObject(this.identify, Settings);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
