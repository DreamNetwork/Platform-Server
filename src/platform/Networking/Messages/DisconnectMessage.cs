using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(0xFFFFFFFF, MessageDirection.ToServer)]
    public sealed class DisconnectMessage : Message
    {
    }
}