using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(1u << 16 | 3u << 8 | 3, MessageDirection.ToServer)]
    public class ChannelDeletePropertyRequest : ChannelRelatedMessage
    {
        public string Name { get; set; }
    }
}