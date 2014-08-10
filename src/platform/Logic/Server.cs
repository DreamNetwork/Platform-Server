using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;

namespace DreamNetwork.PlatformServer.Logic
{
    public abstract class Server
    {
        private readonly ConcurrentBag<Manager> _managers = new ConcurrentBag<Manager>();

        public IEnumerable<Manager> Managers
        {
            get { return _managers; }
        }

        /// <summary>
        ///     Starts the server.
        /// </summary>
        public abstract void Start();

        /// <summary>
        ///     Stops the server.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        ///     Registers a manager with the server. Only one manager of each type can be registered on the same server.
        /// </summary>
        /// <param name="manager">The manager to register.</param>
        public void RegisterManager(Manager manager)
        {
            if (_managers.Any(m => m.GetType() == manager.GetType() || m.GetType().IsSubclassOf(manager.GetType())))
                throw new InvalidOperationException(
                    "A manager of this type is already registered and a second copy is not allowed.");

            if (manager.ServerInstance != null)
                throw new InvalidOperationException("This manager is already assigned to a server.");

            manager.ServerInstance = this;
            _managers.Add(manager);
        }

        /// <summary>
        ///     Gets a specific manager.
        /// </summary>
        /// <typeparam name="T">Requested manager type</typeparam>
        /// <returns>Returns the registered manager instance if found, otherwise default instance ("default(T)").</returns>
        public T GetManager<T>() where T : Manager
        {
            return
                _managers.SingleOrDefault(m => m.GetType() == typeof (T) || m.GetType().IsSubclassOf(typeof (T))) as T;
        }

        /// <summary>
        ///     Handle a message from a client.
        /// </summary>
        /// <param name="sourceClient">Source client</param>
        /// <param name="message">Message to handle</param>
        public void HandleMessage(Client sourceClient, Message message)
        {
            // Preconditions
            if (message.RequestId == 0 && !(message is DisconnectMessage))
                // 0 is reserved for background messages (not assigned to any request)
                throw new InvalidOperationException("Request ID is not valid, 0 is reserved for system messages.");

            foreach (
                var manager in
                    _managers.Where(
                        manager =>
                            !message.HandledByManagers.Contains(manager) &&
                            ForceMessageRouting(manager, sourceClient, message)))
                Debug.WriteLine("{0} handled by {1}", message.GetType(), manager.GetType());
        }

        public bool ForceMessageRouting<T>(Client sourceClient, Message message) where T : Manager
        {
            return ForceMessageRouting(GetManager<T>(), sourceClient, message);
        }

        public bool ForceMessageRouting(Manager manager, Client sourceClient, Message message)
        {
            if (message.HandledByManagers.Contains(manager))
                return false;

            var ret = manager.HandlePacket(sourceClient, message);
            message.HandledByManagers.Add(manager);
            return ret;
        }
    }
}