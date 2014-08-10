using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(3u << 16 | 1u, MessageDirection.ToServer)]
    public class ProfileUpdateRequest : Message
    {
        public Dictionary<string, object> ProfileFields { get; set; }
        public string[] FieldsToDelete { get; set; }
    }
}