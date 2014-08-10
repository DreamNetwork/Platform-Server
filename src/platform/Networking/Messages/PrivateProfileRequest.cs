using System;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(2u << 16 | 1u, MessageDirection.ToServer)]
    public class PrivateProfileRequest : Message
    {
        public Guid ClientGuid { get; set; }
    }
}