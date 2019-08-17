using System;

namespace NsqClient
{
    public class NsqReconnectionEventArgs : EventArgs
    {
        public int Attempts { get; }

        public TimeSpan ReconnectedAfter { get; }
        
        public NsqReconnectionEventArgs(int attempts, TimeSpan interval)
        {
            this.Attempts = attempts;
            this.ReconnectedAfter = interval;
        }
    }
}
