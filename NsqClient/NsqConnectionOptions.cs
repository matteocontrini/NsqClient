namespace NsqClient
{
    public class NsqConnectionOptions
    {
        public string Hostname { get; set; } = "localhost";

        public int Port { get; set; } = 4150;

        public string Topic { get; set; }

        public string Channel { get; set; }

        public int MaxInFlight { get; set; } = 1;
    }
}
