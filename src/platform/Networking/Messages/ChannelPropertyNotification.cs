using System.ComponentModel.Composition;

namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(1u << 16 | 3u << 8 | 0, MessageDirection.ToClient)]
    public class ChannelPropertyNotification : ChannelRelatedMessage
    {
        public string Name { get; set; }
        public bool Deleted { get; set; }
        public object Value { get; set; }
    }
}