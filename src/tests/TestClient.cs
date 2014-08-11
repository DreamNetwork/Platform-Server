using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using DreamNetwork.PlatformServer.Logic;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;

namespace DreamNetwork.PlatformServer.Tests
{
    public class TestClient : Client
    {
        public bool Disconnected;
        public readonly List<Message> SentMessages = new List<Message>();

        private uint _reqId = 1;

        public TestClient()
        {
        }

        public TestClient(Server server)
            : base(server)
        {
            OnConnected();
        }

        public override void ForceDisconnect()
        {
            if (Disconnected)
                return;

            Disconnected = true;
        }
 
        public override void Send(Message message)
        {
            Debug.WriteLine("Client {0}: {1}[0x{2:X8}] from server", Id, message.GetType(), message.MessageTypeId);
            SentMessages.Add(message);
        }

        public void TriggerReceive(Message message)
        {
            message.RequestId = _reqId++;
            Debug.WriteLine("Client {0}: {1}[0x{2:X8}] to server, ID = {3:X8}", Id, message.GetType(), message.MessageTypeId, message.RequestId);
            OnReceivedPacket(message);
        }

        public static TestClient Create(Server server = null)
        {
            var client = server == null
                ? new TestClient { Id = Guid.NewGuid(), Profile = new Dictionary<string, object>() } // constructor will not call base
                : new TestClient(server); // constructor will call base and initialping
            Debug.WriteLine("Client {0}: connected (parent server of type {1})", client.Id, server == null ? "NULL" : server.GetType().Name);
            return client;
        }

        public void Close()
        {
            TriggerReceive(new DisconnectMessage());
        }
    }
}