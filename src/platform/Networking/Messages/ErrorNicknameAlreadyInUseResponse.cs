using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(0xFFu << 16 | 6u, MessageDirection.ToClient)]
    public class ErrorNicknameAlreadyInUseResponse : Message
    {
    }
}