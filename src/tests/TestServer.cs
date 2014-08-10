using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.Logic;
using DreamNetwork.PlatformServer.Logic.Managers;
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

        public new void HandleMessage(Client cl, Message msg)
        {
            Debug.WriteLine("Server handling a message from client {0}", cl.Id);
            base.HandleMessage(cl, msg);
        }

        public static TestServer Create(params Manager[] managers)
        {
            Debug.WriteLine("Creating server with managers {0}", (object)string.Join(", ", managers.Select(m => m.GetType().Name)));
            var s = new TestServer();
            foreach (var m in managers)
            {
                Debug.WriteLine("> Registering manager {0}", m.GetType());
                s.RegisterManager(m);
            }
            return s;
        }

        public static TestServer CreateDefault()
        {
            Debug.WriteLine("Creating server with default managers");
            return Create(
                new ClientManager(),
                new ChannelManager());
        }

        public TestClient CreateClient()
        {
            return TestClient.Create(this);
        }
    }
}