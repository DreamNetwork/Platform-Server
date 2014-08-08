using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 6u, MessageDirection.ToServer)]
    public sealed class ChannelClientGuidListRequest : ChannelRelatedMessage
    {
    }
}