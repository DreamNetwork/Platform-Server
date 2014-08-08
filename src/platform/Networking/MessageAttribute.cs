using System;

namespace DreamNetwork.PlatformServer.Networking
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageAttribute : Attribute
    {
        public MessageAttribute(uint type, MessageDirection directions)
        {
            Type = type;
            Directions = directions;
        }

        public uint Type { get; private set; }

        public MessageDirection Directions { get; private set; }
    }
}