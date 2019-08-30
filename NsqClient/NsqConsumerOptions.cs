using System.Threading;

namespace NsqClient
{
    public class NsqConsumerOptions : NsqClientOptions
    {
        public string Topic { get; set; }

        public string Channel { get; set; }

        public int MaxInFlight { get; set; } = 1;

        public NsqConsumerOptions(string hostname, int port, string topic, string channel)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.Topic = topic;
            this.Channel = channel;
        }
        
        public NsqConsumerOptions(string hostname, int port, string topic, string channel, int maxInFlight)
            : this(hostname, port, topic, channel)
        {
            this.MaxInFlight = maxInFlight;
        }
        
        public NsqConsumerOptions(string topic, string channel)
        {
            this.Topic = topic;
            this.Channel = channel;
        }
        
        public NsqConsumerOptions(string topic, string channel, int maxInFlight)
            : this(topic, channel)
        {
            this.MaxInFlight = maxInFlight;
        }
    }
}
