﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.IO;
using DreamNetwork.PlatformServer.Logic.Managers;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;
using NCalc;

namespace DreamNetwork.PlatformServer.Logic
{
    public class Channel
    {
        private readonly ConcurrentDictionary<Guid, Client> _clients = new ConcurrentDictionary<Guid, Client>();

        public Channel(Client owner, string[] tags, IEnumerable<string> requiredProfileFields = null, string password = null, bool allowBroadcasts = true,
            bool allowClientDiscovery = true, bool allowOwnerDiscovery = true)
        {
            if (owner == null)
                throw new InvalidOperationException();
            if (tags == null)
                throw new InvalidOperationException();
            if (password == string.Empty)
                password = null;

            Debug.WriteLine(
                "New channel instance (owner is {0})",
                owner);
            Id = SequentialGuid.NewGuid();
            Owner = owner;
            Tags = tags;
            Password = password;
            AllowBroadcasts = allowBroadcasts;
            AllowClientDiscovery = allowClientDiscovery;
            AllowOwnerDiscovery = allowOwnerDiscovery;
            RequiredProfileFields = (requiredProfileFields ?? new List<string>()).ToList();
        }

        public Guid Id { get; private set; }

        public string[] Tags { get; private set; }

        public IEnumerable<Client> Clients
        {
            get { return _clients.Values; }
        }

        // TODO: Allow ownership change
        public Client Owner { get; private set; }

        public bool AllowBroadcasts { get; set; }

        public bool AllowClientDiscovery { get; set; }

        public bool AllowOwnerDiscovery { get; set; }

        public bool IsClosed { get { return !Clients.Contains(Owner); } }

        public string Password { get; set; }

        public List<string> RequiredProfileFields { get; private set; }

        private readonly ConcurrentDictionary<string, object> _properties = new ConcurrentDictionary<string, object>();

        public ReadOnlyDictionary<string, object> Properties { get { return new ReadOnlyDictionary<string, object>(_properties); } } 

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
            foreach (var client in Clients)
            {
                client.Send(message, client.Equals(sourceClient) ? request : null);
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
            if (IsClosed && client != Owner)
                return false;

            if (!_clients.TryAdd(client.Id, client))
                return false;

            Broadcast(new ChannelClientJoined {ChannelGuid = Id, ClientGuid = client.Id}, client, request);

            // tell client about all current properties set in the channel
            foreach (var property in Properties)
            {
                client.Send(new ChannelPropertyNotification
                {
                    ChannelGuid = Id,
                    Deleted = false,
                    Name = property.Key,
                    Value = property.Value
                }, request);
            }
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
            Broadcast(leaveMessage, client, request);
            if (!removeSilently)
                client.Send(leaveMessage, request);

            return true;
        }

        public void DeleteProperty(string name, Client sourceClient, Message message)
        {
            object oldValue;
            if (_properties.TryRemove(name, out oldValue))
            {
                Broadcast(new ChannelPropertyNotification {ChannelGuid = Id, Deleted = true, Name = name}, sourceClient,
                    message);
            }
        }

        public void SetProperty(string name, object value, Client sourceClient, Message message)
        {
            // The logic seems too primitive here
            if (_properties.ContainsKey(name))
            {
                if (_properties.TryUpdate(name, value, _properties[name]))
                {
                    Broadcast(
                        new ChannelPropertyNotification {ChannelGuid = Id, Deleted = false, Name = name, Value = value},
                        sourceClient, message);
                }
            }
            else
            {
                if (_properties.TryAdd(name, value))
                {
                    Broadcast(
                        new ChannelPropertyNotification { ChannelGuid = Id, Deleted = false, Name = name, Value = value },
                        sourceClient, message);
                }
            }
        }
    }
}