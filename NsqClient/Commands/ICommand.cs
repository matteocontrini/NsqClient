namespace NsqClient.Commands
{
    public interface ICommand
    {
        byte[] ToBytes();
    }
}
