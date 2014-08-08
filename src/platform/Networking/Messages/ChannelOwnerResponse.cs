using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 8u, MessageDirection.ToClient)]
    public sealed class ChannelOwnerResponse : ChannelRelatedMessage
    {
        public Guid ClientGuid { get; set; }
    }
}