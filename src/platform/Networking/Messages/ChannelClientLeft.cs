using System;
using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 4u, MessageDirection.ToClient)]
    public sealed class ChannelClientLeft : ChannelRelatedMessage
    {
        public Guid ClientGuid { get; set; }
    }
}