using System;

namespace StatusPlatform.Networking
{
    [Flags]
    public enum MessageDirection
    {
        ToServer = 1,
        ToClient = 2,
        Both = 1 | 2
    }
}