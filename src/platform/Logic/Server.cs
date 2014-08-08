using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StatusPlatform.Networking;
using StatusPlatform.Networking.Messages;

namespace StatusPlatform.Logic
{
    public abstract class Server
    {
        private readonly ConcurrentBag<Manager> _managers = new ConcurrentBag<Manager>();

        public IEnumerable<Manager> Managers
        {
            get { return _managers; }
        }

        public abstract void Start();

        public abstract void Stop();

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

        public T GetManager<T>() where T : Manager
        {
            return _managers.SingleOrDefault(m => m.GetType() == typeof(T) || m.GetType().IsSubclassOf(typeof(T))) as T;
        }

        public void HandleMessage(Client sourceClient, Message message)
        {
            // Preconditions
            if (message.RequestId == 0 && !(message is DisconnectMessage)) // 0 is reserved for background messages (not assigned to any request)
                throw new InvalidOperationException("Request ID is not valid, 0 is reserved for system messages.");

            foreach (var manager in _managers.Where(manager => manager.HandlePacket(sourceClient, message)))
                Debug.WriteLine("{0} handled by {1}", message.GetType(), manager.GetType());
        }
    }
}