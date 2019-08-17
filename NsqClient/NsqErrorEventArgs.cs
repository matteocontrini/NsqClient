using System;

namespace NsqClient
{
    public class NsqErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public NsqErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
}
