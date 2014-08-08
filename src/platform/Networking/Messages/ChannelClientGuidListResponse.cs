using System;
using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 6u, MessageDirection.ToClient)]
    public sealed class ChannelClientGuidListResponse : ChannelRelatedMessage
    {
        public Guid[] ClientGuids { get; set; }
    }
}