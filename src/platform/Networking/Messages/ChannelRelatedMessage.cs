using System;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    public class ChannelRelatedMessage : Message
    {
        public Guid ChannelGuid { get; set; }
    }
}