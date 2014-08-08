using DreamNetwork.PlatformServer.Networking;

namespace DreamNetwork.PlatformServer.Logic
{
    public abstract class Manager
    {
        internal Server ServerInstance;

        protected Server Server
        {
            get { return ServerInstance; }
        }

        public abstract bool HandlePacket(Client sourceClient, Message message);
    }
}