using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 1u, MessageDirection.ToServer)]
    public sealed class ChannelDiscoveryRequest : Message
    {
        public string Query { get; set; }
    }
}