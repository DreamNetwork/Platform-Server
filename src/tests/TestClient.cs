using System;
using System.Collections.Generic;
using System.Diagnostics;
using DreamNetwork.PlatformServer.Logic;
using DreamNetwork.PlatformServer.Networking;

namespace DreamNetwork.PlatformServer.Tests
{
    public class TestClient : Client
    {
        public readonly List<Message> SentMessages = new List<Message>();
        public override void ForceDisconnect()
        {
            throw new NotImplementedException();
        }
 
        public override void Send(Message message)
        {
            Debug.WriteLine("Client {0}: {1}[0x{2:X8}] from server", Id, message.GetType(), message.MessageTypeId);
            SentMessages.Add(message);
        }

        public void TriggerReceive(Message message)
        {
            Debug.WriteLine("Client {0}: {1}[0x{2:X8}] to server", Id, message.GetType(), message.MessageTypeId);
            OnReceivedPacket(message);
        }

        public static TestClient Create()
        {
            var client = new TestClient { Id = Guid.NewGuid(), Profile = new Dictionary<string, object>() };
            Debug.WriteLine("Client {0}: connected", client.Id);
            return client;
        }

    }
}