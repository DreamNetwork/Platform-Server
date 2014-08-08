using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u, MessageDirection.ToServer)]
    public sealed class AnonymousLoginRequest : Message
    {
        public Dictionary<string, object> Profile { get; set; }
    }
}