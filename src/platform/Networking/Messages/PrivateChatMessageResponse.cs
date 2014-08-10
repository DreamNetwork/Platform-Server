using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(2u << 16 | 1u << 8 | 1u, MessageDirection.ToClient)]
    public class PrivateChatMessageResponse : Message
    {
        public bool Sent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}