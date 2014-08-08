using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;
using NCalc;

namespace DreamNetwork.PlatformServer.Logic
{
    public class Channel
    {
        private readonly ConcurrentDictionary<Guid, Client> _clients = new ConcurrentDictionary<Guid, Client>();

        public Channel(Client owner, string[] tags, string password = null, bool allowBroadcasts = true,
            bool allowClientDiscovery = true, bool allowOwnerDiscovery = true)
        {
            if (owner == null)
                throw new InvalidOperationException();
            if (tags == null)
                throw new InvalidOperationException();
            if (password == string.Empty)
                password = null;

            Debug.WriteLine(
                "New channel instance (owner is {0}, allowBroadcasts is {1}, allowClientDiscovery is {2}, allowOwnerDiscovery is {3})",
                owner, allowBroadcasts, allowClientDiscovery, allowOwnerDiscovery);
            Id = SequentialGuid.NewGuid();
            Owner = owner;
            Tags = tags;
            Password = password;
            AllowBroadcasts = allowBroadcasts;
            AllowClientDiscovery = allowClientDiscovery;
            AllowOwnerDiscovery = allowOwnerDiscovery;
        }

        public Guid Id { get; set; }

        public string[] Tags { get; private set; }

        public IEnumerable<Client> Clients
        {
            get { return _clients.Values; }
        }

        public Client Owner { get; private set; }

        public bool AllowBroadcasts { get; set; }

        public bool AllowClientDiscovery { get; set; }

        public bool AllowOwnerDiscovery { get; set; }

        public bool IsClosed { get; private set; }

        public string Password { get; set; }

        public bool Match(string query)
        {
            //return query.All(tag =>
            //{
            var expression = new Expression(query, EvaluateOptions.IgnoreCase);
            Debug.WriteLine("Query to channel {0}: {1}", Id, expression.ParsedExpression);

            expression.EvaluateFunction += (name, args) =>
            {
                switch (name.ToLower())
                {
                    case "tag":
                    {
                        // exactly 1 parameter?
                        var parameters = args.EvaluateParameters();
                        if (parameters.Length != 1)
                        {
                            args.Result = false;
                            break;
                        }

                        // parameter a string?
                        var ptag = parameters[0] as string;
                        if (ptag == null)
                        {
                            args.Result = false;
                            break;
                        }

                        args.Result = Tags.Contains(ptag);
                        break;
                    }
                    case "hasclient":
                    {
                        // exactly 1 parameter?
                        var parameters = args.EvaluateParameters();
                        if (parameters.Length != 1)
                        {
                            args.Result = false;
                            break;
                        }

                        // parameter a string?
                        var pguid = parameters[0] as string;
                        if (pguid == null)
                        {
                            args.Result = false;
                            break;
                        }

                        args.Result = _clients.ContainsKey(Guid.Parse(pguid));
                        break;
                    }
                    case "owner":
                    {
                        // exactly 1 parameter?
                        var parameters = args.EvaluateParameters();
                        if (parameters.Length != 1)
                        {
                            args.Result = false;
                            break;
                        }

                        // parameter a string?
                        var pguid = parameters[0] as string;
                        if (pguid == null)
                        {
                            args.Result = false;
                            break;
                        }

                        args.Result = Owner.Id == Guid.Parse(pguid);
                        break;
                    }
                }

                Debug.WriteLine("  Query function eval: {0} = {1}", name, args.Result);
            };

            expression.EvaluateParameter += (name, args) =>
            {
                switch (name.ToLower())
                {
                    case "clients":
                    {
                        args.Result = Clients.Count();
                        args.HasResult = true;
                    }
                        break;
                    case "needspassword":
                    {
                        args.Result = !string.IsNullOrEmpty(Password);
                        args.HasResult = true;
                    }
                        break;
                    case "allowsbroadcast":
                    {
                        args.Result = AllowBroadcasts;
                        args.HasResult = true;
                    }
                        break;
                    case "allowsclientdiscovery":
                    {
                        args.Result = AllowClientDiscovery;
                        args.HasResult = true;
                    }
                        break;
                    case "allowsownerdiscovery":
                    {
                        args.Result = AllowOwnerDiscovery;
                        args.HasResult = true;
                    }
                        break;
                    default:
                    {
                        args.Result = Tags.Contains(name, StringComparer.InvariantCultureIgnoreCase);
                        args.HasResult = true;
                    }
                        break;
                }

                Debug.WriteLine("  Query parameter eval: {0} = {1}", name, args.Result);
            };

            var result = expression.Evaluate();
            Debug.WriteLine("  >> Result = {0} <<", result);
            if (result is bool)
                return (bool) result;
            if (result is int || result is long || result is short)
                return (long) result > 0;
            if (result is uint || result is ulong || result is ushort)
                return (ulong) result != 0;
            return false;
            //});
        }

        public bool Kick(Client client, Client requester = null, Message request = null)
        {
            if (!_clients.ContainsKey(client.Id))
                return false;

            Client tempClient;
            var result = _clients.TryRemove(client.Id, out tempClient);
            if (!result)
                return false;

            var kickMessage = new ChannelClientKicked {ChannelGuid = Id, ClientGuid = client.Id};
            Broadcast(kickMessage, requester, request);
            client.Send(kickMessage);

            return true;
        }

        public void KickAll()
        {
            foreach (var client in _clients.Values)
            {
                Client tempClient;
                _clients.TryRemove(client.Id, out tempClient);
                client.Send(new ChannelClientKicked {ChannelGuid = Id, ClientGuid = client.Id});
            }
        }

        public void Broadcast(Message message, Client sourceClient = null, Message request = null)
        {
            foreach (var client in _clients.Values)
            {
                if (sourceClient != null && client == sourceClient && request != null)
                {
                    client.Send(Message.CloneResponse(request, message));
                    continue;
                }

                client.Send(message);
            }
        }

        public void Broadcast(object content, Client sourceClient = null, Message request = null)
        {
            Broadcast(
                new ChannelBroadcast
                {
                    ChannelGuid = Id,
                    ClientId = sourceClient != null ? sourceClient.Id : Guid.Empty,
                    Content = content
                }, sourceClient, request);
        }

        public bool AddClient(Client client, Message request = null)
        {
            if (IsClosed)
                return false;

            if (!_clients.TryAdd(client.Id, client))
                return false;

            Broadcast(new ChannelClientJoined {ChannelGuid = Id, ClientGuid = client.Id}, client, request);
            return true;
        }

        public bool RemoveClient(Client client, bool removeSilently)
        {
            return RemoveClient(client, null, removeSilently);
        }

        public bool RemoveClient(Client client, Message request = null, bool removeSilently = false)
        {
            Debug.WriteLine("Removing client {0} from channel {1}", client.Id, Id);

            Client tempClient;
            var result = _clients.TryRemove(client.Id, out tempClient);
            if (!result)
                return false;

            var leaveMessage = new ChannelClientLeft {ChannelGuid = Id, ClientGuid = client.Id};
            if (request != null)
                leaveMessage = Message.CloneResponse(request, leaveMessage);
            Broadcast(leaveMessage);
            if (!removeSilently)
                client.Send(leaveMessage);

            // If channel is now ownerless, close the channel
            if (client != Owner)
                return true;
            IsClosed = true;
            KickAll();

            return true;
        }
    }
}