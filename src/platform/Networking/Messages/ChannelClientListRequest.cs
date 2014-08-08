using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 7u, MessageDirection.ToServer)]
    public sealed class ChannelClientListRequest : ChannelRelatedMessage
    {
    }
}