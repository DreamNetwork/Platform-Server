namespace StatusPlatform.Networking.Messages
{
    [Message(0xFFu << 16 | 4, MessageDirection.ToClient)]
    class ErrorChannelPasswordInvalidResponse : Message
    {
    }
}
