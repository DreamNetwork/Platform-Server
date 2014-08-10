using System;

namespace DreamNetwork.PlatformServer.Networking
{
    [Flags]
    public enum MessageDirection : byte
    {
        ToServer = 1,
        ToClient = 2
    }
}