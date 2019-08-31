using System;

namespace NsqClient
{
    public class NsqConsumerOptions : NsqClientOptions
    {
        public string Topic { get; }

        public string Channel { get; }

        public int MaxInFlight { get; internal set; } = 1;

        public TimeSpan MsgTimeout { get; set; } = TimeSpan.FromMinutes(1);

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
        
        public NsqConsumerOptions(string hostname, int port, string topic, string channel, int maxInFlight, TimeSpan msgTimeout)
            : this(hostname, port, topic, channel, maxInFlight)
        {
            this.MsgTimeout = msgTimeout;
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
        
        public NsqConsumerOptions(string topic, string channel, int maxInFlight, TimeSpan msgTimeout)
            : this(topic, channel, maxInFlight)
        {
            this.MsgTimeout = msgTimeout;
        }
    }
}
