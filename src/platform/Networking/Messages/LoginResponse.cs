using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u, MessageDirection.ToClient)]
    public sealed class LoginResponse : Message
    {
        public bool Success { get; set; }
        public Guid ClientGuid { get; set; }
    }
}