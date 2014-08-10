using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.Logic;
using DreamNetwork.PlatformServer.Logic.Managers;
using DreamNetwork.PlatformServer.Networking;
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

            Debug.WriteLine("Channel {0}: created, tags: {1}, owner is client {2}", channel.Id, string.Join("; ", tags),
                owner.Id);
            return channel;
        }

        [Test]
        public void ChannelCreation()
        {
            var client = TestClient.Create();
            var tags = new[] {"test1", "test2"};
            const string pw = "1234";
            var channel = new Channel(client, tags, new string[0], pw, true, false, false);

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
        public void ChannelDiscovery()
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
            Assert.IsTrue(channelManager.HandleTestMessage(mainClient,
                new ChannelDiscoveryRequest {Query = "tag('test2')"}));
            Assert.IsTrue(mainClient.SentMessages.Any(m => m is ChannelDiscoveryResponse));
            var response = mainClient.SentMessages.Last(m => m is ChannelDiscoveryResponse) as ChannelDiscoveryResponse;
            Assert.IsNotNull(response);
            Debug.WriteLine("Comparison:");
            foreach (var guid in response.ChannelGuids)
                Debug.WriteLine("\t{0}", guid);
            CollectionAssert.AreEquivalent(
                expectedChannels.Where(c => c.Tags.Contains("test2")).Select(c => c.Id).ToArray(), response.ChannelGuids);

            // Test "clients = 3"
            Assert.IsTrue(channelManager.HandleTestMessage(mainClient,
                new ChannelDiscoveryRequest {Query = "clients = 3"}));
            Assert.IsTrue(mainClient.SentMessages.Any(m => m is ChannelDiscoveryResponse));
            response = mainClient.SentMessages.Last(m => m is ChannelDiscoveryResponse) as ChannelDiscoveryResponse;
            Assert.IsNotNull(response);
            Debug.WriteLine("Comparison:");
            foreach (var guid in response.ChannelGuids)
                Debug.WriteLine("\t{0}", guid);
            CollectionAssert.AreEquivalent(
                expectedChannels.Where(c => c.Clients.Count() == 3).Select(c => c.Id).ToArray(), response.ChannelGuids);

            // Test "clients > 3"
            Assert.IsTrue(channelManager.HandleTestMessage(mainClient,
                new ChannelDiscoveryRequest {Query = "clients > 3"}));
            Assert.IsTrue(mainClient.SentMessages.Any(m => m is ChannelDiscoveryResponse));
            response = mainClient.SentMessages.Last(m => m is ChannelDiscoveryResponse) as ChannelDiscoveryResponse;
            Assert.IsNotNull(response);
            Debug.WriteLine("Comparison:");
            foreach (var guid in response.ChannelGuids)
                Debug.WriteLine("\t{0}", guid);
            CollectionAssert.AreEquivalent(
                expectedChannels.Where(c => c.Clients.Count() > 3).Select(c => c.Id).ToArray(), response.ChannelGuids);
        }

        [Test]
        public void ChannelJoin()
        {
            var client = TestClient.Create();
            var channel = CreateTestChannel(client);

            Assert.IsTrue(channel.AddClient(client));
            Assert.IsTrue(channel.Clients.Contains(client));
            Assert.IsTrue(
                client.SentMessages.Any(
                    m => m is ChannelClientJoined && ((ChannelClientJoined) m).ClientGuid == client.Id));
        }

        [Test]
        public void ChannelLeave()
        {
            var client = TestClient.Create();
            var channel = CreateTestChannel(client);
            channel.AddClient(client);

            Assert.IsTrue(channel.RemoveClient(client));
            Assert.IsFalse(channel.Clients.Contains(client));
            Assert.IsTrue(
                client.SentMessages.Any(m => m is ChannelClientLeft && ((ChannelClientLeft) m).ClientGuid == client.Id));
        }

        [Test]
        public void ChannelManagerOpen()
        {
            var client = TestClient.Create();
            var channelManager = new TestChannelManager();

            Assert.IsTrue(channelManager.HandleTestMessage(client,
                new ChannelOpenRequest
                {
                    AllowBroadcasts = true,
                    AllowClientDiscovery = true,
                    AllowOwnerClientDiscovery = true,
                    ChannelPassword = "1234",
                    Tags = new[] {"test1"}
                }));
            Assert.IsTrue(
                client.SentMessages.Any(
                    m => m is ChannelClientJoined && ((ChannelClientJoined) m).ClientGuid == client.Id));
            Assert.IsTrue(channelManager.Channels.Count == 1);
            Assert.IsTrue(channelManager.Channels.First().Clients.Contains(client));
        }

        [Test]
        public void ChannelCloseOwnerLeave()
        {
            var s = TestServer.Create(new ClientManager(), new TestChannelManager());

            var oclient = s.CreateClient();
            Assert.IsTrue(oclient.SentMessages.Any(m => m is InitialPingMessage));
            oclient.TriggerReceive(new AnonymousLoginRequest());
            Assert.IsTrue(oclient.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success));
            var testchannel = s.GetManager<TestChannelManager>().AddChannel(oclient);
            Assert.IsNotNull(testchannel);

            var kclient = s.CreateClient();
            Assert.IsTrue(kclient.SentMessages.Any(m => m is InitialPingMessage));
            kclient.TriggerReceive(new AnonymousLoginRequest());
            Assert.IsTrue(kclient.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success));
            kclient.TriggerReceive(new ChannelJoinRequest
            {
                ChannelGuid = testchannel.Id
            });
            Assert.IsTrue(kclient.SentMessages.Any(m => m is ChannelClientJoined && (m as ChannelClientJoined).ClientGuid.Equals(kclient.Id)));
            oclient.Close();

            Assert.IsTrue(testchannel.IsClosed);
            var kick = kclient.SentMessages.SingleOrDefault(m => m is ChannelClientKicked);
            //kick = kick == default(Message) ? null : kick;
            Assert.IsNotNull(kick);
            var leave = kclient.SentMessages.SingleOrDefault(m => m is ChannelClientLeft);
            Assert.IsNotNull(leave);
            Assert.IsTrue(kclient.SentMessages.IndexOf(leave) < kclient.SentMessages.IndexOf(kick));
        }

        [Test]
        public void ChannelProfileFieldRequirementJoinValid()
        {
            var s = TestServer.Create(new ClientManager(), new TestChannelManager());
            var testchannel = s.GetManager<TestChannelManager>().AddChannel(null, null, new[] { "testy" });

            var c = s.CreateClient();
            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");

            c.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "testy", true } } });
            Assert.IsTrue(c.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success), "Server did not accept valid login request.");

            c.TriggerReceive(new ChannelJoinRequest
            {
                ChannelGuid = testchannel.Id
            });
            Assert.IsTrue(c.SentMessages.Any(m => m is ChannelClientJoined && (m as ChannelClientJoined).ClientGuid.Equals(c.Id)), "Server did not accept valid join request.");
        }

        [Test]
        public void ChannelProfileFieldRequirementJoinInvalid()
        {
            var s = TestServer.Create(new ClientManager(), new TestChannelManager());
            var testchannel = s.GetManager<TestChannelManager>().AddChannel(null, null, new[]{"testx"});

            var c = s.CreateClient();
            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");

            c.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "testy", true } } }); // testy != testx
            Assert.IsTrue(c.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success), "Server did not accept valid login request.");

            c.TriggerReceive(new ChannelJoinRequest
            {
                ChannelGuid = testchannel.Id
            });
            Assert.IsFalse(c.SentMessages.Any(m => m is ChannelClientJoined && (m as ChannelClientJoined).ClientGuid.Equals(c.Id)), "Server accepted invalid join request.");
        }

        [Test]
        public void ClientAnonymousLoginEmptyNickname()
        {
            var s = TestServer.CreateDefault();
            var c = s.CreateClient();
            c.TriggerReceive(
                new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Nickname", "" } } });
            //Debug.WriteLine("{0}", c.SentMessages.LastOrDefault());

            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");
            Assert.IsTrue(c.SentMessages.Any(m => !(m is InitialPingMessage)), "No response from server at all except initial ping");
            Assert.IsTrue(c.SentMessages.Any(m => m is ErrorInvalidNicknameResponse), "Server accepted empty nickname.");
        }

        [Test]
        public void ClientAnonymousLoginInvalidTypeNickname()
        {
            var s = TestServer.CreateDefault();
            var c = s.CreateClient();
            c.TriggerReceive(
                new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Nickname", 80 } } });
            //Debug.WriteLine("{0}", c.SentMessages.LastOrDefault());

            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");
            Assert.IsTrue(c.SentMessages.Any(m => !(m is InitialPingMessage)), "No response from server at all except initial ping");
            Assert.IsTrue(c.SentMessages.Any(m => m is ErrorInvalidNicknameResponse),
                "Server accepted non-string nickname.");
        }

        [Test]
        public void ClientAnonymousLoginNickname()
        {
            var s = TestServer.CreateDefault();
            var c = s.CreateClient();
            c.TriggerReceive(
                new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Nickname", "Testy" } } });
            //Debug.WriteLine("{0}", c.SentMessages.LastOrDefault());

            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");
            Assert.IsTrue(c.SentMessages.Any(m => !(m is InitialPingMessage)), "No response from server at all except initial ping");
            Assert.IsTrue(c.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success),
                "Server did not accept valid nickname.");
        }

        [Test]
        public void ClientAnonymousLoginNicknameDuplicate()
        {
            var server = TestServer.CreateDefault();

            var c1 = server.CreateClient();
            c1.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Nickname", "Testy" } } });
            Assert.IsTrue(c1.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");
            Assert.IsTrue(c1.SentMessages.Any(m => !(m is InitialPingMessage)), "No response from server at all except initial ping");
            Assert.IsTrue(c1.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success));

            var c2 = server.CreateClient();
            c2.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Nickname", "Testy" } } });
            Assert.IsTrue(c2.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");
            Assert.IsTrue(c2.SentMessages.Any(m => !(m is InitialPingMessage)), "No response from server at all except initial ping");
            Assert.IsTrue(c2.SentMessages.Any(m => m is ErrorNicknameAlreadyInUseResponse));
        }

        [Test]
        public void ClientAnonymousLoginNullNickname()
        {
            var server = TestServer.CreateDefault();
            var c = server.CreateClient();
            c.TriggerReceive(new AnonymousLoginRequest {Profile = new Dictionary<string, object> {{"Nickname", null}}});

            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");
            Assert.IsTrue(c.SentMessages.Any(m => !(m is InitialPingMessage)), "No response from server at all except initial ping");
            Assert.IsTrue(c.SentMessages.Any(m => m is ErrorInvalidNicknameResponse), "Server accepted null nickname.");
        }

        [Test]
        public void ClientAnonymousLoginProfileNull()
        {
            var server = TestServer.CreateDefault();
            var c = server.CreateClient();
            c.TriggerReceive(new AnonymousLoginRequest { Profile = null });

            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");
            Assert.IsTrue(c.SentMessages.Any(m => !(m is InitialPingMessage)), "No response from server at all except initial ping");
            Assert.IsTrue(c.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success), "No login response from server");
            Assert.IsFalse(c.Id == Guid.Empty, "No GUID assigned");
            Assert.IsNotNull(c.Profile, "Profile is NULL");
            Assert.IsEmpty(c.Profile, "Profile is not empty");
        }

        [Test]
        public void ClientProfileUpdateRemoveChangeConflict()
        {
            var s = TestServer.CreateDefault();
            var c = s.CreateClient();
            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");

            c.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Testfield", true } } });
            Assert.IsTrue(c.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success), "Server did not accept valid nickname.");

            c.TriggerReceive(new ProfileUpdateRequest
            {
                FieldsToDelete = new[] {"Testfield"},
                ProfileFields = new Dictionary<string, object> {{"Testfield", true}}
            });
            Assert.IsTrue(c.SentMessages.Any(m => m is ProfileUpdateResponse && !(m as ProfileUpdateResponse).Success), "Server accepted invalid profile update.");
        }

        [Test]
        public void ClientProfileUpdateRemove()
        {
            var s = TestServer.CreateDefault();
            var c = s.CreateClient();
            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");

            c.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Testfield", true } } });
            Assert.IsTrue(c.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success), "Server did not accept valid nickname.");

            c.TriggerReceive(new ProfileUpdateRequest
            {
                FieldsToDelete = new[] {  "Testfield" }
            });
            Assert.IsTrue(c.SentMessages.Any(m => m is ProfileUpdateResponse && (m as ProfileUpdateResponse).Success), "Server did not accept valid profile field removal.");
            Assert.IsFalse(c.Profile.ContainsKey("Testfield"), "Test field still contained in profile even though server reported successful removal");
        }

        [Test]
        public void ClientProfileUpdateRemoveNonExistent()
        {
            // I think it's logical that removed fields should simply be ignored in removal requests

            var s = TestServer.CreateDefault();
            var c = s.CreateClient();
            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");

            c.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object>() });
            Assert.IsTrue(c.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success), "Server did not accept anonymous login");

            c.TriggerReceive(new ProfileUpdateRequest
            {
                FieldsToDelete = new[] { "Testfield" }
            });
            Assert.IsTrue(c.SentMessages.Any(m => m is ProfileUpdateResponse && (m as ProfileUpdateResponse).Success), "Server did not accept valid (non-existent) profile field removal.");
        }

        [Test]
        public void ClientProfileUpdateNickname()
        {
            var s = TestServer.CreateDefault();
            var c = s.CreateClient();

            Assert.IsTrue(c.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");

            c.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Nickname", "Testy" } } });
            Assert.IsTrue(c.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success), "Server did not accept valid nickname.");

            c.TriggerReceive(new ProfileUpdateRequest
            {
                ProfileFields = new Dictionary<string, object> {{"Nickname", "Testy2"}}
            });
            Assert.IsTrue(c.SentMessages.Any(m => m is ProfileUpdateResponse && (m as ProfileUpdateResponse).Success), "Server did not accept valid nickname change.");
        }

        [Test]
        public void ClientProfileUpdateNicknameDuplicate()
        {
            var server = TestServer.CreateDefault();

            var c1 = server.CreateClient();
            Assert.IsTrue(c1.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");

            c1.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Nickname", "Testy2" } } });
            Assert.IsTrue(c1.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success));

            var c2 = server.CreateClient();
            Assert.IsTrue(c2.SentMessages.Any(m => m is InitialPingMessage), "No response from server at all");

            c2.TriggerReceive(new AnonymousLoginRequest { Profile = new Dictionary<string, object> { { "Nickname", "Testy" } } });
            Assert.IsTrue(c2.SentMessages.Any(m => m is LoginResponse && (m as LoginResponse).Success), "Server did not accept valid nickname.");

            c2.TriggerReceive(new ProfileUpdateRequest
            {
                ProfileFields = new Dictionary<string, object> { { "Nickname", "Testy2" } }
            });
            Assert.IsTrue(c2.SentMessages.Any(m => m is ProfileUpdateResponse && !(m as ProfileUpdateResponse).Success), "Server accepted dupliate nickname change.");
        }
    }
}