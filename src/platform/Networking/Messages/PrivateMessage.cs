using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(2u << 16 | 1u << 8 | 0u, MessageDirection.ToClient)]
    public class PrivateMessage : Message
    {
        /// <summary>
        ///     Source user
        /// </summary>
        public Guid ClientGuid { get; set; }

        public object Content { get; set; }

        public DateTime Timestamp { get; set; }
    }
}