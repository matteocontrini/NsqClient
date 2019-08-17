using System;

namespace NsqClient
{
    public class NsqDisconnectionEventArgs : EventArgs
    {
        public bool WillReconnect { get; }

        public NsqDisconnectionEventArgs(bool willReconnect)
        {
            this.WillReconnect = willReconnect;
        }
    }
}
