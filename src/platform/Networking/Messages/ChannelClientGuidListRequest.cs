using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 6u, MessageDirection.ToServer)]
    public sealed class ChannelClientGuidListRequest : ChannelRelatedMessage
    {
    }
}