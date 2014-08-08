using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 1u, MessageDirection.ToServer)]
    public sealed class ChannelDiscoveryRequest : Message
    {
        public string Query { get; set; }
    }
}