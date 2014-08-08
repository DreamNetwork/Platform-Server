using System.ComponentModel.Composition;

namespace StatusPlatform.Networking.Messages
{
    [Export(typeof(Message))]
    [Message(1u << 16 | 1u << 8 | 1u, MessageDirection.ToServer)]
    public class ChannelChatMessageRequest : ChannelRelatedMessage
    {
        public string Message { get; set; }
    }
}