using StatusPlatform.Networking;

namespace StatusPlatform.Logic
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