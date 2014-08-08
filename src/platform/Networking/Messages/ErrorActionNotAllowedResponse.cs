using System;
using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(0xFFu << 16 | 3u, MessageDirection.ToClient)]
    public class ErrorActionNotAllowedResponse : Message
    {
        public uint SourceMessageType { get; set; }
        public Guid ChannelGuid { get; set; }
    }
}