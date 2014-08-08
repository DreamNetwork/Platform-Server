using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 2u, MessageDirection.ToServer)]
    public sealed class ChannelJoinRequest : ChannelRelatedMessage
    {
        public string ChannelPassword { get; set; }
    }
}