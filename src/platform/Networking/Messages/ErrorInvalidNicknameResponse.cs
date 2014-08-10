using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(0xFFu << 16 | 7u, MessageDirection.ToClient)]
    public class ErrorInvalidNicknameResponse : Message
    {
    }
}