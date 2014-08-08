using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 7u, MessageDirection.ToServer)]
    public sealed class ChannelClientListRequest : ChannelRelatedMessage
    {
    }
}