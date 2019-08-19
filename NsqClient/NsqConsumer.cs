using System;
using System.Threading.Tasks;

namespace NsqClient
{
    public class NsqConsumer : INsqConsumer
    {
        private readonly NsqConnection conn;
        
        public NsqConsumer(NsqConsumerOptions options)
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

        public event EventHandler<NsqMessageEventArgs> OnMessage
        {
            add => this.conn.OnMessage += value;
            remove => this.conn.OnMessage -= value;
        }

        public Task ConnectAsync()
        {
            return this.conn.FirstConnect();
        }

        public void Dispose()
        {
            this.conn.Close();
        }
    }
}
