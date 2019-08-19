using System;
using System.Threading.Tasks;

namespace NsqClient
{
    public class NsqProducer : INsqProducer
    {
        private readonly NsqConnection conn;
        
        public NsqProducer(NsqProducerOptions options)
        {
            this.conn = new NsqConnection(options);
        }

        public event EventHandler<NsqErrorEventArgs> OnError
        {
            add => this.conn.OnError += value;
            remove => this.conn.OnError -= value;
        }
        
        public event EventHandler<NsqDisconnectionEventArgs> OnDisconnected
        {
            add => this.conn.OnDisconnected += value;
            remove => this.conn.OnDisconnected -= value;
        }
        
        public event EventHandler<NsqReconnectionEventArgs> OnReconnected
        {
            add => this.conn.OnReconnected += value;
            remove => this.conn.OnReconnected -= value;
        }

        public Task ConnectAsync()
        {
            return this.conn.FirstConnect();
        }

        public Task PublishAsync(string topicName, string body)
        {
            return this.conn.Publish(topicName, body);
        }

        public Task PublishAsync(string topicName, byte[] body)
        {
            return this.conn.Publish(topicName, body);
        }

        public void Dispose()
        {
            this.conn.Close();
        }
    }
}
