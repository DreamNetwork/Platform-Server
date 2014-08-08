using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 5u, MessageDirection.ToClient)]
    public sealed class ChannelBroadcast : ChannelRelatedMessage
    {
        public Guid ClientId { get; set; }
        public object Content { get; set; }
    }
}