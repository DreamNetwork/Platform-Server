using System;
using System.Diagnostics;
using System.Linq;
using Fleck2.Interfaces;
using StatusPlatform.Logic;
using StatusPlatform.Networking.Messages;

namespace StatusPlatform.Networking.Servers
{
    public class WebsocketClient : Client
    {
        private IWebSocketConnection _connection;

        public WebsocketClient(Server server, IWebSocketConnection connection)
            : base(server)
        {
            _connection = connection;
            _connection.OnError = error => Debug.WriteLine(error.ToString());
            _connection.OnBinary = data =>
            {
                var msg = Message.Deserialize(MessageDirection.ToServer, data);
                Debug.WriteLine("{0} sent message to us: {1} (0x{2:X8})", connection.ConnectionInfo.ClientIpAddress,
                    msg != null ? msg.GetType().Name.Split('.').Last() : "!! UNKNOWN !!",
                    msg != null ? msg.MessageTypeId : BitConverter.ToUInt32(data, 0));
                if (msg != null)
                    foreach (var p in msg.GetType().GetProperties())
                    {
                        Debug.WriteLine("> {0} = {1}", p.Name, p.GetValue(msg));
                    }
                OnReceivedPacket(msg);
            };
            _connection.OnClose = () =>
            {
                _connection = null;
                Debug.WriteLine("{0} disconnected from Websocket", (object) connection.ConnectionInfo.ClientIpAddress);
                OnReceivedPacket(new DisconnectMessage());
            };

            Debug.WriteLine("{0} connected to Websocket", (object) connection.ConnectionInfo.ClientIpAddress);
            OnConnected();
        }

        public override void Send(Message message)
        {
            if (_connection == null)
                return;
            try
            {
                _connection.Send(message.Serialize());
                Debug.WriteLine("{0} received message from us: {1} (0x{2:X8})",
                    _connection.ConnectionInfo.ClientIpAddress,
                    message.GetType().Name.Split('.').Last(), message.MessageTypeId);
            }
            catch
            {
                Debug.WriteLine("{0} did NOT receive message from us: {1} (0x{2:X8})",
                    _connection.ConnectionInfo.ClientIpAddress,
                    message.GetType().Name.Split('.').Last(), message.MessageTypeId);
            }
            foreach (var p in message.GetType().GetProperties())
            {
                Debug.WriteLine("> {0} = {1}", p.Name, p.GetValue(message));
            }
        }

        public override void ForceDisconnect()
        {
            if (_connection != null)
            {
                var connection = _connection;
                _connection = null;
                connection.Close();
            }
        }
    }
}