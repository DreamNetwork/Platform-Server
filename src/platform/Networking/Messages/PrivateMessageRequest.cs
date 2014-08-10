using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(2u << 16 | 1u << 8 | 1u, MessageDirection.ToServer)]
    public class PrivateMessageRequest : Message
    {
        /// <summary>
        ///     Target user
        /// </summary>
        public Guid ClientGuid { get; set; }

        public object Content { get; set; }
    }
}