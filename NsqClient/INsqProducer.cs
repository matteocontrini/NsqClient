using System.Threading.Tasks;

namespace NsqClient
{
    public interface INsqProducer : INsqClient
    {
        Task PublishAsync(string topicName, string body);
        Task PublishAsync(string topicName, byte[] body);
    }
}
