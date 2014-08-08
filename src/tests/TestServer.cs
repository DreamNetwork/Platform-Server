using StatusPlatform.Logic;
using StatusPlatform.Networking;

namespace StatusPlatformTest
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