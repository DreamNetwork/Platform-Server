using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;
using NUnit.Framework;

namespace DreamNetwork.PlatformServer.Tests
{
    [TestFixture]
    public class NetworkTests
    {
        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void NetworkMessageRefuseSystemRequestId()
        {
            var server = new TestServer();
            server.HandleMessage(TestClient.Create(), new ChannelChatMessage());
        }

        [Test]
        public void NetworkMessageNoRefuseDisconnectSystemRequestId()
        {
            var server = new TestServer();
            server.HandleMessage(TestClient.Create(), new DisconnectMessage());
        }

        [Test]
        public void NetworkMessageTypeTest()
        {
            Assert.AreEqual(typeof (ChannelBroadcast),
                Message.GetMessageTypeById(MessageDirection.ToClient, 1u << 16 | 5u));
        }

        [Test]
        public void NetworkMessageDeserializationTest()
        {
            // that's an AnonymousLoginRequest sample with empty profile, generated in the browser
            var buffer = new byte[]
            {
                0, 0, 0, 1, // request id
                0, 0, 0, 1, // message type
                19, 0, 0, 0, 3, 80, 114, 111, 102, 105, 108, 101, 0, 5, 0, 0, 0, 0, 0 // bson-encoded message body
            };

            var msg = Message.Deserialize(MessageDirection.ToServer, buffer);
            Assert.IsTrue(msg is AnonymousLoginRequest);
            Assert.AreEqual((msg as AnonymousLoginRequest).Profile.Count, 0);
        }

        [Test]
        public void NetworkMessageConversationSerializationTest()
        {
            var msgs = new Message[]
            {
                new AnonymousLoginRequest
                {
                    Profile = new Dictionary<string, object>()
                },
                new LoginResponse
                {
                    ClientGuid = Guid.NewGuid(),
                    Success = true
                },
                new ChannelDiscoveryRequest
                {
                    Query = "true"
                },
                new ChannelDiscoveryResponse
                {
                    ChannelGuids = new[]
                    {
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        Guid.NewGuid()
                    }
                },
                new ChannelJoinRequest {ChannelGuid = Guid.NewGuid()},
                new ChannelClientJoined {ChannelGuid = Guid.NewGuid(), ClientGuid = Guid.NewGuid()}
            };
            foreach (var msg in msgs)
            {
                Console.WriteLine(msg.GetType());
                CheckNetworkMessageSerialization(msg);
            }
        }

        private void CheckNetworkMessageSerialization(Message originalMessage)
        {
            var mattr =
                originalMessage.GetType().GetCustomAttributes(typeof (MessageAttribute), false).Single() as
                    MessageAttribute;
            Assert.IsNotNull(mattr);

            var sw = new Stopwatch();

            sw.Start();
            var serializedMessage = originalMessage.Serialize();
            sw.Stop();
            PrintUtils.HexDisplay(serializedMessage, "Serialized message, took" + sw.Elapsed.TotalSeconds + " sec");

            sw.Reset();
            sw.Start();
            var message = Message.Deserialize(mattr.Directions, serializedMessage);
            sw.Stop();
            Debug.WriteLine("Deserialized message, took " + sw.Elapsed.TotalSeconds + " sec");

            Assert.AreEqual(originalMessage.MessageTypeId, message.MessageTypeId, "Deserialized type ID mismatch");
            Assert.AreEqual(originalMessage.GetType(), message.GetType(), "Deserialized type mismatch");
            //Assert.AreEqual(originalMessage, message, "Deserialized content mismatch");
        }
    }
}