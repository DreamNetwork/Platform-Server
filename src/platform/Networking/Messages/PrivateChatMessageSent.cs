using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(2u << 16 | 2u << 8 | 1u, MessageDirection.ToClient)]
    public class PrivateChatMessageSent : Message
    {
        public DateTime Timestamp { get; set; }
    }
}