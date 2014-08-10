using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(0xFFu << 16 | 2u, MessageDirection.ToClient)]
    public class ErrorChannelNotFoundResponse : Message
    {
        public Guid ChannelGuid { get; set; }
    }
}