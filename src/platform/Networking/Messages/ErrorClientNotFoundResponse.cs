using System;
using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(0xFFu << 16 | 1u, MessageDirection.ToClient)]
    public class ErrorClientNotFoundResponse : Message
    {
        public Guid ClientGuid { get; set; }
    }
}