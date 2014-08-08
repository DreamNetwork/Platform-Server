using DreamNetwork.PlatformServer.Logic;
using DreamNetwork.PlatformServer.Networking;

namespace DreamNetwork.PlatformServer.Tests
{
    public class TestServer : Server {
        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public void HandleMessagePublic(Client cl, Message msg)
        {
            HandleMessage(cl, msg);
        }
    }
}