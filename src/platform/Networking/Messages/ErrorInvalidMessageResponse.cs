using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(0xFFu << 16 | 5u, MessageDirection.ToClient)]
    public class ErrorInvalidMessageResponse : Message
    {
    }
}