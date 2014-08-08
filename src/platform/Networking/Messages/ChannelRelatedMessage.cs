using System;

namespace StatusPlatform.Networking.Messages
{
    public class ChannelRelatedMessage : Message
    {
        public Guid ChannelGuid { get; set; }
    }
}