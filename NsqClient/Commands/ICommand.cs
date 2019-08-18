namespace NsqClient.Commands
{
    internal interface ICommand
    {
        byte[] ToBytes();
    }
}
