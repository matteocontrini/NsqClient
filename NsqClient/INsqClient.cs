using System;
using System.Threading.Tasks;

namespace NsqClient
{
    public interface INsqClient : IDisposable
    {
        event EventHandler<NsqErrorEventArgs> OnError;
        event EventHandler<NsqDisconnectionEventArgs> OnDisconnected;
        event EventHandler<NsqReconnectionEventArgs> OnReconnected;
        Task ConnectAsync();
    }
}
