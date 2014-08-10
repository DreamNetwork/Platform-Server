﻿using System;
using System.Linq;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;

namespace DreamNetwork.PlatformServer.Logic.Managers
{
    public class ChatManager : Manager
    {
        private ChannelManager _chmInstance;
        private ClientManager _clmInstance;

        private ChannelManager ChannelManager
        {
            get
            {
                if (_chmInstance != null)
                    return _chmInstance;

                return _chmInstance = Server.GetManager<ChannelManager>();
            }
        }

        private ClientManager ClientManager
        {
            get
            {
                if (_clmInstance != null)
                    return _clmInstance;

                return _clmInstance = Server.GetManager<ClientManager>();
            }
        }

        public override bool HandlePacket(Client sourceClient, Message message)
        {
            // Client needs to be authenticated
            if (sourceClient.Id == Guid.Empty)
                return false;

            // Private message request
            if (message is PrivateMessageRequest)
            {
                var request = message as PrivateMessageRequest;
                var targetClient = ClientManager.Clients.SingleOrDefault(c => c.Id == request.ClientGuid);
                if (targetClient == default(Client))
                {
                    sourceClient.Send(new PrivateMessageResponse { Sent = false }, message);
                    sourceClient.Send(new ErrorClientNotFoundResponse(), message);
                    return false;
                }

                var timestamp = DateTime.UtcNow;
                targetClient.Send(
                    new PrivateMessage
                    {
                        Timestamp = timestamp,
                        ClientGuid = sourceClient.Id,
                        Content = request.Content
                    }, message);
                sourceClient.Send(new PrivateMessageResponse {Sent=true, Timestamp = timestamp}, message);
                return true;
            }

            // Channel message request
            if (message is ChannelChatMessageRequest)
            {
                var request = message as ChannelChatMessageRequest;
                var channel = ChannelManager.Channels.SingleOrDefault(c => c.Id == request.ChannelGuid);
                if (channel == default(Channel))
                {
                    sourceClient.Send(new ErrorClientNotFoundResponse(), message);
                    return false;
                }

                if (!channel.AllowBroadcasts)
                {
                    sourceClient.Send(new ErrorActionNotAllowedResponse(), message);
                }

                var timestamp = DateTime.UtcNow;
                channel.Broadcast(
                    new ChannelChatMessage
                    {
                        Timestamp = timestamp,
                        ClientGuid = sourceClient.Id,
                        Message = request.Message
                    }, sourceClient, message);
                return true;
            }

            return false;
        }
    }
}