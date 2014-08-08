using System;
using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 1u, MessageDirection.ToClient)]
    public sealed class ChannelDiscoveryResponse : Message
    {
        public Guid[] ChannelGuids { get; set; }
    }
}