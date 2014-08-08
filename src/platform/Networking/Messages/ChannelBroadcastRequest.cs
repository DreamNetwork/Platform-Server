using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 5u, MessageDirection.ToServer)]
    public sealed class ChannelBroadcastRequest : ChannelRelatedMessage
    {
        public object Content { get; set; }
    }
}