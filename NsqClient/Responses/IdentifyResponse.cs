using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NsqClient.Frames;

namespace NsqClient.Responses
{
    class IdentifyResponse
    {
        private static readonly JsonSerializerSettings Settings;

        static IdentifyResponse()
        {
            Settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        public int MaxRdyCount { get; set; }

        public string Version { get; set; }

        public int MaxMsgTimeout { get; set; }

        public int MsgTimeout { get; set; }

        public bool TlsV1 { get; set; }

        public bool Deflate { get; set; }

        public int DeflateLevel { get; set; }

        public int MaxDeflateLevel { get; set; }

        public bool Snappy { get; set; }

        public int SampleRate { get; set; }

        public bool AuthRequired { get; set; }

        public int OutputBufferSize { get; set; }

        public int OutputBufferTimeout { get; set; }
        
        public static IdentifyResponse ParseWithFrame(ResponseFrame frame)
        {
            return JsonConvert.DeserializeObject<IdentifyResponse>(frame.Message, Settings);
        }
    }
}
