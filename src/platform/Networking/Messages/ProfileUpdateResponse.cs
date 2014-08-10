namespace DreamNetwork.PlatformServer.Networking.Messages
{
    [Message(3u << 16 | 1u, MessageDirection.ToClient)]
    public class ProfileUpdateResponse : Message
    {
        public bool Success { get; set; }
        public string[] FailedFields { get; set; }
    }
}