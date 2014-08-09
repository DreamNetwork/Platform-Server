using System;
using System.Collections.Generic;
using System.Diagnostics;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Messages;

namespace DreamNetwork.PlatformServer.Logic
{
    public abstract class Client
    {
        protected Client()
        {
        }

        protected Client(Server server)
        {
            Server = server;
            Id = Guid.Empty;
            Profile = new Dictionary<string, object>();
        }

        public Server Server { get; private set; }

        public Guid Id { get; set; }

        public Dictionary<string, object> Profile { get; set; }

        public abstract void Send(Message message);

        public void Send(Message response, Message request)
        {
            Send(request == null ? response : Message.CloneResponse(request, response));
        }

        public abstract void ForceDisconnect();

        protected void OnReceivedPacket(Message message)
        {
            if (ReceivedPacket != null)
                ReceivedPacket(this, message);

            Server.HandleMessage(this, message);

            // Close connection actively on disconnect message
            if (message is DisconnectMessage)
            {
                try
                {
                    ForceDisconnect();
                }
                catch (Exception error)
                {
                    Debug.WriteLine("Error while forcing client disconnection. {0}", error);
                }
            }
        }

        public event Action<Client, Message> ReceivedPacket;

        protected void OnConnected()
        {
            Send(new InitialPingMessage());

            if (Connected != null)
                Connected(this);
        }

        public event Action<Client> Connected;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Client) obj);
        }

        private bool Equals(Client other)
        {
            return Equals(Server, other.Server) && Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Server != null ? Server.GetHashCode() : 0)*397) ^ Id.GetHashCode();
            }
        }
    }
}