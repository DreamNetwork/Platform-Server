using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Fleck;
using StatusPlatform.Logic;

namespace StatusPlatform.Networking.Servers
{
    public class WebsocketServer : Server
    {
        private readonly List<WebSocketServer> _servers = new List<WebSocketServer>();

        public override void Start()
        {
            _servers.Add(new WebSocketServer("ws://0.0.0.0:28110/"));
            if (File.Exists("server.pfx"))
                _servers.Add(new WebSocketServer("wss://0.0.0.0:28111/") { Certificate = new X509Certificate2("server.pfx") });

            foreach (var server in _servers)
                server.Start(c =>
                {
                    c.OnOpen = () => new WebsocketClient(this, c);
                });
        }

        public override void Stop()
        {
            foreach (var server in _servers)
                server.Dispose();
            _servers.Clear();
        }
    }
}
