using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 2u, MessageDirection.ToClient)]
    public sealed class ChannelClientJoined : ChannelRelatedMessage
    {
        public Guid ClientGuid { get; set; }
    }
}