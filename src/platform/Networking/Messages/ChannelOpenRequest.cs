using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 3u, MessageDirection.ToServer)]
    public class ChannelOpenRequest : Message
    {
        public string ChannelPassword { get; set; }
        public string[] Tags { get; set; }
        public string[] NeededNonUniqueProfileFields { get; set; }
        public string[] NeededUniqueProfileFields { get; set; }
        public bool AllowBroadcasts { get; set; }
        public bool AllowOwnerClientDiscovery { get; set; }
        public bool AllowClientDiscovery { get; set; }
        public byte ExpiryMinutes { get; set; }
    }
}