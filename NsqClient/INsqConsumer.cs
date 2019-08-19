using System;

namespace NsqClient
{
    public interface INsqConsumer : INsqClient
    {
        event EventHandler<NsqMessageEventArgs> OnMessage;
    }
}
