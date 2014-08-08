using System.ComponentModel.Composition;
using StatusPlatform.Logic;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 7u, MessageDirection.ToClient)]
    public sealed class ChannelClientListResponse : ChannelRelatedMessage
    {
        public Client[] Clients { get; set; }
    }
}