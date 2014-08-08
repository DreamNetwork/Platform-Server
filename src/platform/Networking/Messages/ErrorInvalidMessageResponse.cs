using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(0xFFu << 16 | 5u, MessageDirection.ToClient)]
    public class ErrorInvalidMessageResponse : Message
    {
    }
}