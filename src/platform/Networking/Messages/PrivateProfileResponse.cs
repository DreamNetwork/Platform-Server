using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(2u << 16 | 1u, MessageDirection.ToClient)]
    public class PrivateProfileResponse : Message
    {
        public Dictionary<string, string> Profile { get; set; }
    }
}