namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Message(0xFFu << 16 | 4, MessageDirection.ToClient)]
    internal class ErrorChannelPasswordInvalidResponse : Message
    {
    }
}