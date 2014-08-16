using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(1u << 16 | 3u << 8 | 1, MessageDirection.ToServer)]
    public class ChannelSetPropertyRequest : ChannelRelatedMessage
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}