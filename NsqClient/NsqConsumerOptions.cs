namespace NsqClient
{
    public class NsqConsumerOptions : NsqClientOptions
    {
        public string Topic { get; set; }

        public string Channel { get; set; }

        public int MaxInFlight { get; set; } = 1;
    }
}
