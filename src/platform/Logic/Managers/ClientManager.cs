using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using StatusPlatform.Networking;
using StatusPlatform.Networking.Messages;

namespace StatusPlatform.Logic.Managers
{
    public class ClientManager : Manager
    {
        private readonly ConcurrentDictionary<Guid, Client> _clients = new ConcurrentDictionary<Guid, Client>();

        public IEnumerable<Client> Clients { get { return _clients.Values; } }

        public void RegisterClient(Client client)
        {
            client.ReceivedPacket += (cl, msg) => HandlePacket(cl, msg);
        }

        public override bool HandlePacket(Client sourceClient, Message message)
        {
            // TODO: introduce some sort of authentication

            // Client wants to log in anonymously
            if (message is AnonymousLoginRequest)
            {
                // Client already logged in?
                if (sourceClient.Id != Guid.Empty)
                    return false;

                var req = message as AnonymousLoginRequest;
                sourceClient.Id = SequentialGuid.NewGuid();
                sourceClient.Profile = req.Profile;
                if (!_clients.TryAdd(sourceClient.Id, sourceClient))
                {
                    sourceClient.Send(new LoginResponse { ClientGuid = Guid.Empty, Success = false }, message);
                    return false;
                }
                sourceClient.Send(new LoginResponse { ClientGuid = sourceClient.Id, Success = true }, message);
                return true;
            }

            // Client disconnected (this Message can be both triggered by the client or by the server)
            if (message is DisconnectMessage)
            {
                Client tempClient;
                return _clients.TryRemove(sourceClient.Id, out tempClient);
            }

            return false;
        }
    }
}