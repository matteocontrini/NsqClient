namespace NsqClient
{
    public class NsqProducerOptions : NsqClientOptions
    {
        public NsqProducerOptions()
        {
        }
        
        public NsqProducerOptions(string hostname, int port)
        {
            this.Hostname = hostname;
            this.Port = port;
        }
    }
}
