using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(1u << 16 | 1u << 8 | 1u, MessageDirection.ToClient)]
    public class ChannelChatMessage : ChannelRelatedMessage
    {
        /// <summary>
        /// Source user
        /// </summary>
        public Guid ClientGuid { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
