using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;

namespace DreamNetwork.PlatformServer.Logic.Managers
{
    public class ChannelManager : Manager
    {
        private readonly ConcurrentDictionary<Guid, Channel> _channels = new ConcurrentDictionary<Guid, Channel>();

        public ICollection<Channel> Channels
        {
            get { return _channels.Values; }
        }

        public override bool HandlePacket(Client sourceClient, Message message)
        {
            // client must be authenticated to use any channel functionality
            if (sourceClient.Id == Guid.Empty)
                return false;

            // Client disconnected (this Message can be both triggered by the client or by the server)
            if (message is DisconnectMessage)
            {
                foreach (var channel in Channels.Where(c => c.Clients.Contains(sourceClient)).ToArray())
                {
                    channel.RemoveClient(sourceClient, true);
                    if (channel.IsClosed)
                        RemoveChannel(channel);
                }
                return true;
            }

            // channel-related Message?
            var chanPacket = message as ChannelRelatedMessage;
            if (chanPacket != null)
            {
                // TODO: allow owner to close down channel

                if (!_channels.ContainsKey(chanPacket.ChannelGuid))
                {
                    Debug.WriteLine(
                        "Received channel-related message for non-existant channel (Type: {0} [0x{1:X8}], Channel: {2})",
                        message.GetType(), message.MessageTypeId, chanPacket.ChannelGuid);
                    sourceClient.Send(new ErrorChannelNotFoundResponse(), message);
                    return false;
                }
                var channel = _channels[chanPacket.ChannelGuid];

                // Broadcast requests
                if (chanPacket is ChannelBroadcastRequest)
                {
                    if (!channel.AllowBroadcasts && sourceClient != channel.Owner)
                    {
                        sourceClient.Send(new ErrorActionNotAllowedResponse(), message);
                        return false;
                    }
                    channel.Broadcast((chanPacket as ChannelBroadcastRequest).Content, sourceClient, message);
                    return true;
                }

                // Join requests
                if (chanPacket is ChannelJoinRequest)
                {
                    if (channel.IsClosed)
                    {
                        // TODO: notify client that channel is closed
                        sourceClient.Send(new ErrorActionNotAllowedResponse(), message);
                        return false;
                    }

                    if (!channel.RequiredProfileFields.TrueForAll(sourceClient.Profile.ContainsKey))
                    {
                        // TODO: notify client about which profile fields are missing
                        sourceClient.Send(new ErrorActionNotAllowedResponse(), message);
                        return false;
                    }

                    var joinRequest = chanPacket as ChannelJoinRequest;
                    if (channel.Password != null && joinRequest.ChannelPassword != channel.Password)
                    {
                        sourceClient.Send(new ErrorChannelPasswordInvalidResponse(), message);
                        return false;
                    }

                    return channel.AddClient(sourceClient, message);
                }

                // Leave requests
                if (chanPacket is ChannelLeaveRequest)
                {
                    if (!channel.RemoveClient(sourceClient, message))
                    {
                        sourceClient.Send(new ErrorActionNotAllowedResponse(), message);
                        // TODO: notification here exactly as well even though that should only occur extremely rarely
                        return false;
                    }

                    // TODO: Make 100% sure the channel gets removed
                    return !channel.IsClosed || RemoveChannel(channel);
                }

                // Client discovery requests
                if (chanPacket is ChannelClientListRequest)
                {
                    if (!channel.AllowClientDiscovery)
                    {
                        sourceClient.Send(new ErrorActionNotAllowedResponse(), message);
                        return false;
                    }
                    sourceClient.Send(new ChannelClientListResponse
                    {
                        ChannelGuid = channel.Id,
                        Clients = channel.Clients.ToArray()
                    }, message);
                    return true;
                }

                // Client guid-only discovery requests
                if (chanPacket is ChannelClientGuidListRequest)
                {
                    if (!channel.AllowClientDiscovery)
                    {
                        sourceClient.Send(new ErrorActionNotAllowedResponse(), message);
                        return false;
                    }
                    sourceClient.Send(new ChannelClientGuidListResponse
                    {
                        ChannelGuid = channel.Id,
                        ClientGuids = channel.Clients.Select(c => c.Id).ToArray()
                    }, message);
                    return true;
                }

                // Owner discovery requests
                if (chanPacket is ChannelOwnerRequest)
                {
                    if (!channel.AllowOwnerDiscovery)
                    {
                        sourceClient.Send(new ErrorActionNotAllowedResponse(), message);
                        return false;
                    }
                    sourceClient.Send(
                        new ChannelOwnerResponse {ChannelGuid = channel.Id, ClientGuid = channel.Owner.Id}, message);
                    return true;
                }

                Debug.WriteLine("Unhandled channel-related message of type {1} (0x{0:X8})!!", message.MessageTypeId,
                    message.GetType());
                return false;
            }

            // Client wants to find channels
            if (message is ChannelDiscoveryRequest)
            {
                var req = message as ChannelDiscoveryRequest;
                if (req.Query == null)
                {
                    sourceClient.Send(new ErrorInvalidMessageResponse(), message);
                    return false;
                }
                sourceClient.Send(new ChannelDiscoveryResponse
                {
                    ChannelGuids =
                        _channels.Values.Where(c => c.Match(req.Query))
                            .Distinct()
                            .Select(c => c.Id)
                            .ToArray()
                }, message);
                return true;
            }

            // Client wants to create a channel
            if (message is ChannelOpenRequest)
            {
                var req = message as ChannelOpenRequest;

                Channel channel = null;
                do
                {
                    if (channel != null)
                        Debug.WriteLine("Channel creation led to ID collision! (Channel: {0})", channel.Id);

                    channel = new Channel(sourceClient, req.Tags, req.RequiredProfileFields, req.ChannelPassword, req.AllowBroadcasts,
                        req.AllowClientDiscovery, req.AllowOwnerClientDiscovery);
                } while (_channels.ContainsKey(channel.Id));

                if (!AddChannel(channel))
                {
                    // TODO: notify client that channel creation failed
                    sourceClient.Send(new ErrorActionNotAllowedResponse(), message);
                    return false;
                }

                return channel.AddClient(sourceClient, message);
            }

            return false;
        }

        protected virtual bool AddChannel(Channel channel)
        {
            Debug.WriteLine("Adding channel {0}", channel.Id);
            return _channels.TryAdd(channel.Id, channel);
        }

        protected virtual bool RemoveChannel(Channel channel)
        {
            Debug.WriteLine("Removing channel {0}", channel.Id);
            channel.KickAll();
            Channel tempChannel;
            return _channels.TryRemove(channel.Id, out tempChannel);
        }
    }
}