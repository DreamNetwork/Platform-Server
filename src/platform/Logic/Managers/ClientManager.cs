using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;

namespace DreamNetwork.PlatformServer.Logic.Managers
{
    public class ClientManager : Manager
    {
        private readonly ConcurrentDictionary<Guid, Client> _clients = new ConcurrentDictionary<Guid, Client>();

        public IEnumerable<Client> Clients
        {
            get { return _clients.Values; }
        }

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

                // Normalize profile (for dumb inputs)
                if (req.Profile == null)
                    req.Profile = new Dictionary<string, object>();

                // Has nickname?
                if (req.Profile.ContainsKey("Nickname"))
                {
                    if (!(req.Profile["Nickname"] is string) // nickname not a string
                        || string.IsNullOrEmpty(req.Profile["Nickname"] as string) // nickname empty/null
                        )
                    {
                        sourceClient.Send(new ErrorInvalidNicknameResponse(), message);
                        return false;
                    }

                    // Anyone with this unique nickname?
                    if (
                        Clients.Any(
                            c => c.Profile.ContainsKey("Nickname") && c.Profile["Nickname"] == req.Profile["Nickname"]))
                    {
                        sourceClient.Send(new ErrorNicknameAlreadyInUseResponse(), message);
                        return false;
                    }
                }

                sourceClient.Id = SequentialGuid.NewGuid();
                sourceClient.Profile = req.Profile;
                if (!_clients.TryAdd(sourceClient.Id, sourceClient))
                {
                    sourceClient.Send(new LoginResponse {ClientGuid = Guid.Empty, Success = false}, message);
                    return false;
                }
                sourceClient.Send(new LoginResponse {ClientGuid = sourceClient.Id, Success = true}, message);
                return true;
            }

            // Client disconnected (this Message can be both triggered by the client or by the server)
            if (message is DisconnectMessage)
            {
                Client tempClient;
                return _clients.TryRemove(sourceClient.Id, out tempClient);
            }

            // From here on only handle messages from authenticated clients
            if (sourceClient.Id == Guid.Empty)
                return false;

            // Profile updates
            if (message is UpdateProfileFieldsRequest)
            {
                var req = message as UpdateProfileFieldsRequest;
                
                // detect fields which are requested to be both set and removed.
                // that's a logic conflict and therefore we should just drop the request in that case.
                var failedFields = req.FieldsToDelete == null || req.ProfileFields == null
                    ? new List<string>()
                    : req.FieldsToDelete.Where(req.ProfileFields.ContainsKey).ToList();

                // changes
                if (req.ProfileFields != null)
                    foreach (var i in req.ProfileFields.Where(i => !failedFields.Contains(i.Key)))
                    {
                        switch (i.Key)
                        {
                            case "Nickname":
                                // Anyone with this unique nickname?
                                if (!(i.Value is string) // nickname not a string
                                    || string.IsNullOrEmpty(i.Value as string) // nickname empty/null
                                    ||
                                    Clients.Any(
                                        c =>
                                            !ReferenceEquals(c, sourceClient) && c.Profile.ContainsKey("Nickname") &&
                                            c.Profile["Nickname"] == i.Value)
                                    )
                                {
                                    failedFields.Add(i.Key);
                                    break;
                                }
                                goto default;
                            default:
                                sourceClient.Profile[i.Key] = i.Value;
                                Debug.WriteLine("Client {0}: Profile field {1} set to {2}", sourceClient.Id, i.Key, i.Value);
                                break;
                        }
                    }

                // removal
                if (req.FieldsToDelete != null)
                    foreach (var fieldName in req.FieldsToDelete
                        .Where(i => sourceClient.Profile.ContainsKey(i) && !failedFields.Contains(i))
                        .ToArray()) // ToArray to avoid "changed while processing" situation
                    {
                        if (!sourceClient.Profile.Remove(fieldName))
                        {
                            failedFields.Add(fieldName);
                            continue;
                        }
                        Debug.WriteLine("Client {0}: Profile field {1} deleted", sourceClient.Id, fieldName);
                    }

                // result
                var success = !failedFields.Any();
                sourceClient.Send(new UpdateProfileFieldsResponse
                {
                    FailedFields = failedFields.ToArray(),
                    Success = success
                }, message);
                return success;
            }

            return false;
        }
    }
}