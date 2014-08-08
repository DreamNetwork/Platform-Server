using System.ComponentModel.Composition;
using DreamNetwork.PlatformServer.Logic;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 7u, MessageDirection.ToClient)]
    public sealed class ChannelClientListResponse : ChannelRelatedMessage
    {
        public Client[] Clients { get; set; }
    }
}