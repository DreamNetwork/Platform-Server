using System;
using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(2u << 16 | 2u << 8 | 1u, MessageDirection.ToClient)]
    public class PrivateChatMessageSent : Message
    {
        public DateTime Timestamp { get; set; }
    }
}