using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(2u << 16 | 1u << 8 | 1u, MessageDirection.ToClient)]
    public class PrivateChatMessage : Message
    {
        /// <summary>
        /// Source user
        /// </summary>
        public Guid ClientGuid { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}