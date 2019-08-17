using System;
using System.Text;
using System.Threading.Tasks;
using NsqClient.Commands;
using NsqClient.Frames;

namespace NsqClient
{
    public class NsqMessageEventArgs : EventArgs
    {
        private NsqConnection connection;
        private MessageFrame frame;

        public ushort Attempt => this.frame.Attempts;

        public byte[] Body => this.frame.MessageBody;
        
        public string BodyString => Encoding.UTF8.GetString(this.Body);

        internal NsqMessageEventArgs(NsqConnection connection, MessageFrame frame)
        {
            this.connection = connection;
            this.frame = frame;
        }

        public Task Finish()
        {
            return this.connection.WriteProtocolCommand(new FinishCommand(this.frame.MessageId));
        }
        
        public Task Requeue(TimeSpan delay = default)
        {
            if (delay == default)
            {
                delay = TimeSpan.FromSeconds(1);
            }

            int milliseconds = Convert.ToInt32(delay.TotalMilliseconds);
            
            return this.connection.WriteProtocolCommand(new RequeueCommand(this.frame.MessageId, milliseconds));
        }
        
        public Task Touch()
        {
            return this.connection.WriteProtocolCommand(new TouchCommand(this.frame.MessageId));
        }
    }
}
