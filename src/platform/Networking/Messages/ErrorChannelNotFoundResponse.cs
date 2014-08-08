using System;
using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(0xFFu << 16 | 2u, MessageDirection.ToClient)]
    public class ErrorChannelNotFoundResponse : Message
    {
        public Guid ChannelGuid { get; set; }
    }
}