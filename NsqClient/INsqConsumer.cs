using System;
using System.Threading.Tasks;

namespace NsqClient
{
    public interface INsqConsumer : INsqClient
    {
        event EventHandler<NsqMessageEventArgs> OnMessage;

        Task SetMaxInFlight(int value);
    }
}
