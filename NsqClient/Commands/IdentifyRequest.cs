namespace NsqClient.Commands
{
    internal class IdentifyRequest
    {
        public string ClientId { get; set; }
        
        public string Hostname { get; set; }

        public bool FeatureNegotiation { get; set; }

        public int? HeartbeatInterval { get; set; }

        public int? OutputBufferSize { get; set; }

        public int? OutputBufferTimeout { get; set; }

        public bool? TlsV1 { get; set; }

        public bool? Snappy { get; set; }

        public bool? Deflate { get; set; }

        public int? DeflateLevel { get; set; }

        public int? SampleRate { get; set; }
        
        public string UserAgent { get; set; }

        public int? MsgTimeout { get; set; }
    }
}
