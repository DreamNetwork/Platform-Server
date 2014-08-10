using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof (Message))]
    [Message(1u << 16 | 3u, MessageDirection.ToServer)]
    public class ChannelOpenRequest : Message
    {
        public string ChannelPassword { get; set; }
        public string[] Tags { get; set; }
        public string[] RequiredProfileFields { get; set; }
        public bool AllowBroadcasts { get; set; }
        public bool AllowOwnerClientDiscovery { get; set; }
        public bool AllowClientDiscovery { get; set; }
        // TODO: Implement channel expiry after owner leaves (?)
        public byte ExpiryMinutes { get; set; }
    }
}