namespace NsqClient
{
    public class NsqClientOptions
    {
        public string Hostname { get; protected set; } = "localhost";

        public int Port { get; protected set; } = 4150;
    }
}
