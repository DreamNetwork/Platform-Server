using System;
using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 9u, MessageDirection.ToClient)]
    public sealed class ChannelClientKicked : ChannelRelatedMessage
    {
        public Guid ClientGuid { get; set; }
    }
}