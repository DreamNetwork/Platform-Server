using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.Logic;
using DreamNetwork.PlatformServer.Networking.Messages;
using NUnit.Framework;

namespace DreamNetwork.PlatformServer.Tests
{
    [TestFixture]
    public class LogicTests
    {
        private static Channel CreateTestChannel(Client owner, string[] tags = null)
        {
            var channel = new Channel(
                owner,
                tags = tags ?? new string[0]);
            
            Debug.WriteLine("Channel {0}: created, tags: {1}, owner is client {2}", channel.Id, string.Join("; ", tags), owner.Id);
            return channel;
        }

        [Test]
        public void ChannelCreationTest()
        {
            var client = TestClient.Create();
            var tags = new[] {"test1", "test2"};
            const string pw = "1234";
            var channel = new Channel(client, tags, pw, true, false, false);

            Assert.AreSame(channel.Owner, client);
            Assert.IsFalse(channel.Clients.Any());
            Assert.IsNotNull(channel.Id);
            Assert.IsFalse(channel.AllowClientDiscovery);
            Assert.IsFalse(channel.AllowOwnerDiscovery);
            Assert.IsTrue(channel.AllowBroadcasts);
            Assert.AreSame(channel.Password, pw);
            Assert.AreSame(channel.Tags, tags);
        }

        [Test]
        public void ChannelJoinTest()
        {
            var client = TestClient.Create();
            var channel = CreateTestChannel(client);
            
            Assert.IsTrue(channel.AddClient(client));
            Assert.IsTrue(channel.Clients.Contains(client));
            Assert.IsTrue(client.SentMessages.Any(m => m is ChannelClientJoined && ((ChannelClientJoined)m).ClientGuid == client.Id));
        }

        [Test]
        public void ChannelLeaveTest()
        {
            var client = TestClient.Create();
            var channel = CreateTestChannel(client);
            channel.AddClient(client);

            Assert.IsTrue(channel.RemoveClient(client));
            Assert.IsFalse(channel.Clients.Contains(client));
            Assert.IsTrue(client.SentMessages.Any(m => m is ChannelClientLeft && ((ChannelClientLeft)m).ClientGuid == client.Id));
        }

        [Test]
        public void ChannelManagerOpenTest()
        {
            var client = TestClient.Create();
            var channelManager = new TestChannelManager();

            Assert.IsTrue(channelManager.HandleTestMessage(client, new ChannelOpenRequest{AllowBroadcasts=true,AllowClientDiscovery=true,AllowOwnerClientDiscovery=true,ChannelPassword="1234",Tags=new []{"test1"}}));
            Assert.IsTrue(client.SentMessages.Any(m => m is ChannelClientJoined && ((ChannelClientJoined)m).ClientGuid == client.Id));
            Assert.IsTrue(channelManager.Channels.Count == 1);
            Assert.IsTrue(channelManager.Channels.First().Clients.Contains(client));
        }

        [Test]
        public void ChannelDiscoveryTest()
        {
            var channelManager = new TestChannelManager();

            // This client will do the discovery
            var mainClient = TestClient.Create();

            // This client will be used to create channels.
            // Note that all freshly created channels will have this client, so by expectation at least 1 client is always in a new channel.
            var ownerClient = TestClient.Create();

            // Collection of channels we will compare results to
            var expectedChannels = new List<Channel>
            {
                channelManager.AddChannel(ownerClient, new[] {"test1"}), // will contain owner only (1 client)
                channelManager.AddChannel(ownerClient, new[] {"test2"}), // will contain owner only (1 client)
                channelManager.AddChannel(ownerClient, new[] {"test1"}), // code below will fill this to 3 clients
                channelManager.AddChannel(ownerClient, new[] {"test2"}) // code below will fill this to 4 clients
            };

            // Make clients join the channels so we can do maths as well
            Assert.IsTrue(expectedChannels[2].AddClient(TestClient.Create()));
            Assert.IsTrue(expectedChannels[2].AddClient(TestClient.Create()));
            Assert.IsTrue(expectedChannels[3].AddClient(TestClient.Create()));
            Assert.IsTrue(expectedChannels[3].AddClient(TestClient.Create()));
            Assert.IsTrue(expectedChannels[3].AddClient(TestClient.Create()));

            // Test for tag "test2"
            Assert.IsTrue(channelManager.HandleTestMessage(mainClient, new ChannelDiscoveryRequest { Query = "tag('test2')" }));
            Assert.IsTrue(mainClient.SentMessages.Any(m => m is ChannelDiscoveryResponse));
            var response = mainClient.SentMessages.Last(m => m is ChannelDiscoveryResponse) as ChannelDiscoveryResponse;
            Assert.IsNotNull(response);
            Debug.WriteLine("Comparison:");
            foreach (var guid in response.ChannelGuids)
                Debug.WriteLine("\t{0}", guid);
            CollectionAssert.AreEquivalent(expectedChannels.Where(c => c.Tags.Contains("test2")).Select(c => c.Id).ToArray(), response.ChannelGuids);

            // Test "clients = 3"
            Assert.IsTrue(channelManager.HandleTestMessage(mainClient, new ChannelDiscoveryRequest { Query = "clients() = 3" }));
            Assert.IsTrue(mainClient.SentMessages.Any(m => m is ChannelDiscoveryResponse));
            response = mainClient.SentMessages.Last(m => m is ChannelDiscoveryResponse) as ChannelDiscoveryResponse;
            Assert.IsNotNull(response);
            Debug.WriteLine("Comparison:");
            foreach (var guid in response.ChannelGuids)
                Debug.WriteLine("\t{0}", guid);
            CollectionAssert.AreEquivalent(expectedChannels.Where(c => c.Clients.Count() == 3).Select(c => c.Id).ToArray(), response.ChannelGuids);

            // Test "clients > 3"
            Assert.IsTrue(channelManager.HandleTestMessage(mainClient, new ChannelDiscoveryRequest { Query = "clients > 3" }));
            Assert.IsTrue(mainClient.SentMessages.Any(m => m is ChannelDiscoveryResponse));
            response = mainClient.SentMessages.Last(m => m is ChannelDiscoveryResponse) as ChannelDiscoveryResponse;
            Assert.IsNotNull(response);
            Debug.WriteLine("Comparison:");
            foreach (var guid in response.ChannelGuids)
                Debug.WriteLine("\t{0}", guid);
            CollectionAssert.AreEquivalent(expectedChannels.Where(c => c.Clients.Count() > 3).Select(c => c.Id).ToArray(), response.ChannelGuids);

        }
    }
}