using System;

namespace NsqClient
{
    public class NsqOnErrorEventArgs : EventArgs
    {
        public string Message { get; }

        public NsqOnErrorEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
