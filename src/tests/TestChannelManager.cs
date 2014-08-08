using System.Diagnostics;
using System.Linq;
using StatusPlatform.Logic;
using StatusPlatform.Logic.Managers;
using StatusPlatform.Networking;
using StatusPlatform.Networking.Messages;

namespace StatusPlatformTest
{
    public class TestChannelManager : ChannelManager
    {
        public bool HandleTestMessage(Client client, Message message)
        {
            Debug.WriteLine("ChannelManager: {1}[0x{2:X8}] from client {0}", client.Id, message.GetType(), message.MessageTypeId);
            return HandlePacket(client, message);
        }

        public Channel AddChannel(TestClient owner = null, string[] tags = null)
        {
            if (owner == null)
                Debug.WriteLine("ChannelManager: creating channel with new dummy user");
            owner = owner ?? TestClient.Create();
            HandleTestMessage(owner,
                new ChannelOpenRequest
                {
                    AllowBroadcasts = true,
                    AllowClientDiscovery = true,
                    AllowOwnerClientDiscovery = true,
                    Tags = tags ?? new string[0]
                });
            return Channels.Single(c => c.Id == ((ChannelClientJoined) owner.SentMessages.Last(m => m is ChannelClientJoined)).ChannelGuid);
        }

        protected override bool AddChannel(Channel channel)
        {
            Debug.WriteLine("ChannelManager: adding channel {0}", channel.Id);
            return base.AddChannel(channel);
        }

        protected override bool RemoveChannel(Channel channel)
        {
            Debug.WriteLine("ChannelManager: removing channel {0}", channel.Id);
            return base.AddChannel(channel);
        }
    }
}