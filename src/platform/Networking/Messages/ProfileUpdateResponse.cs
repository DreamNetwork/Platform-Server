using System;
using System.Collections.Generic;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Message(3u << 16 | 1u, MessageDirection.ToClient)]
    public class ProfileUpdateResponse : Message
    {
        public bool Success { get; set; }
        public string[] FailedFields { get; set; }
    }

    [Message(3u << 16 | 0u, MessageDirection.ToClient)]
    public class ProfileUpdate : Message
    {
        public Guid ClientGuid { get; set; }
        public Dictionary<string, object> ProfileFields { get; set; } 
    }
}