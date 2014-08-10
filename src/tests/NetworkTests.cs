using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;
using MsgPack;
using NUnit.Framework;

namespace DreamNetwork.PlatformServer.Tests
{
    public class NetworkTests
    {
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
            PrintUtils.HexDisplay(serializedMessage, "Serialized message, took " + sw.Elapsed.TotalSeconds + " sec");

            sw.Reset();
            sw.Start();
            var message = Message.Deserialize(mattr.Directions, serializedMessage);
            sw.Stop();
            Debug.WriteLine("Deserialized message, took " + sw.Elapsed.TotalSeconds + " sec");

            Assert.AreEqual(originalMessage.MessageTypeId, message.MessageTypeId, "Deserialized type ID mismatch");
            Assert.AreEqual(originalMessage.GetType(), message.GetType(), "Deserialized type mismatch");
            foreach (var p in originalMessage.GetType().GetProperties())
            {
                var value = p.GetValue(originalMessage, null);
                var deserializedProperty = message.GetType().GetProperty(p.Name);
                var deserializedValue = deserializedProperty.GetValue(message, null);
                var deserializedPropertyType = deserializedValue == null ? deserializedProperty.PropertyType : deserializedValue.GetType();

                Assert.AreEqual(p, deserializedProperty,
                    "Deserialized message property mismatch (field: {0})", p.Name);

                if (deserializedValue is MessagePackObject)
                {
                    var packedValue = (MessagePackObject) deserializedValue;
                    Assert.IsTrue(packedValue.IsTypeOf(p.PropertyType).GetValueOrDefault());
                    if (packedValue.IsRaw)
                    {
                        CollectionAssert.AreEqual(value as byte[], packedValue.AsBinary(),
                            "Deserialized message content binary mismatch (field: {0})", p.Name);
                    }
                    else if (packedValue.IsArray)
                    {
                        // TODO: Is this even correct?
                        CollectionAssert.AreEqual(value as Array, packedValue.ToObject() as Array,
                            "Deserialized message content array mismatch (field: {0})", p.Name);
                    }
                    else
                    {
                        Assert.Inconclusive(
                            "Can't determine whether field {0} has been deserialized correctly, MessagePackObject type test not implemented yet.",
                            p.Name);
                    }
                } else if (deserializedPropertyType.IsArray)
                    CollectionAssert.AreEqual(value as Array, deserializedValue as Array,
                        "Deserialized message content array mismatch (field: {0})", p.Name);
                else
                    Assert.AreEqual(value, deserializedValue,
                        "Deserialized message content mismatch (field: {0})", p.Name);
            }
        }

        [Test]
        public void NetworkMessageConversationSerialization()
        {
            var rand = new Random();
            var randomContent = new byte[1024];
            rand.NextBytes(randomContent);

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
                new ChannelClientJoined {ChannelGuid = Guid.NewGuid(), ClientGuid = Guid.NewGuid()},
                new ChannelBroadcast {ChannelGuid = Guid.NewGuid(), ClientId = Guid.NewGuid(), Content = randomContent}
            };
            foreach (var msg in msgs)
            {
                Console.WriteLine(msg.GetType());
                CheckNetworkMessageSerialization(msg);
            }
        }

        [Test]
        public void NetworkMessageDuplicates()
        {
            var foundMessages = new Dictionary<uint, MessageDirection>();
            foreach (
                var msgType in
                    typeof (Message).Assembly.GetTypes()
                        .Where(
                            m =>
                                m.IsSubclassOf(typeof (Message)) &&
                                m.GetCustomAttributes(typeof (MessageAttribute), false).Any()))
            {
                var msgAttr = msgType.GetCustomAttributes(typeof (MessageAttribute), false).Single() as MessageAttribute;
                Assert.IsNotNull(msgAttr);
                if (foundMessages.ContainsKey(msgAttr.Type) && foundMessages[msgAttr.Type].HasFlag(msgAttr.Directions))
                {
                    Assert.Fail("Message type 0x{0:X8} with direction {1} is defined more than once.", msgAttr.Type,
                        msgAttr.Directions);
                }
                if (foundMessages.ContainsKey(msgAttr.Type) && msgAttr.Directions.HasFlag(foundMessages[msgAttr.Type]))
                {
                    Assert.Fail("Message type 0x{0:X8} with direction {1} is defined more than once.", msgAttr.Type,
                        foundMessages[msgAttr.Type]);
                }
                Debug.WriteLine("0x{0:X8} {1} = {2}", msgAttr.Type, msgAttr.Directions, msgType.FullName);
                if (!foundMessages.ContainsKey(msgAttr.Type))
                    foundMessages.Add(msgAttr.Type, msgAttr.Directions);
                else
                    foundMessages[msgAttr.Type] |= msgAttr.Directions;
            }
        }

        [Test]
        public void NetworkMessageNoRefuseDisconnectSystemRequestId()
        {
            var server = new TestServer();
            server.HandleMessage(TestClient.Create(), new DisconnectMessage());
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void NetworkMessageRefuseSystemRequestId()
        {
            var server = new TestServer();
            server.HandleMessage(TestClient.Create(), new ChannelBroadcastRequest()
                /* Request ID is set to default which is 0 */);
        }

        [Test]
        public void NetworkMessageType()
        {
            Assert.AreEqual(typeof (ChannelBroadcast),
                Message.GetMessageTypeById(MessageDirection.ToClient, 1u << 16 | 5u));
        }
    }
}