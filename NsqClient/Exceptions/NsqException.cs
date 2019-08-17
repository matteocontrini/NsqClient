using System;

namespace NsqClient.Exceptions
{
    public class NsqException : Exception
    {
        public NsqException(string message) : base(message)
        {
        }

        public NsqException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
