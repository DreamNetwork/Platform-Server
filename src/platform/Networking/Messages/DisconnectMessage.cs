using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(0xFFFFFFFF, MessageDirection.ToServer)]
    public sealed class DisconnectMessage : Message
    {
    }
}