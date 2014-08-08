using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using DreamNetwork.PlatformServer.Logic.Managers;
using DreamNetwork.PlatformServer.Networking;
using DreamNetwork.PlatformServer.Networking.Servers;

namespace DreamNetwork.PlatformServer
{
    static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Contains("typeids.js"))
            {
                var dict = new Dictionary<string, Dictionary<string, uint>>
                {
                    {"client", new Dictionary<string, uint>()},
                    {"server", new Dictionary<string, uint>()},
                    {"both", new Dictionary<string, uint>()}
                };
                foreach (
                    var messageType in
                        Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(
                                t =>
                                    t.IsSubclassOf(typeof (Message)) && 
                                    t.GetCustomAttributes(typeof(MessageAttribute), false).Any()))
                {
                    var a = messageType.GetCustomAttributes(typeof(MessageAttribute), false).Single() as MessageAttribute;
                    dict[a.Directions == MessageDirection.ToServer ? "client" : a.Directions == MessageDirection.ToClient ? "server" : "both"].Add(messageType.Name, a.Type);
                }
                foreach (var i in dict["both"])
                {
                    dict["client"].Add(i.Key, i.Value);
                    dict["server"].Add(i.Key, i.Value);
                }
                dict.Remove("both");
                var js = new StringBuilder();
                js.AppendLine("var typeIds = {");
                foreach (var i in dict)
                {
                    js.AppendFormat("\t\"{0}\": {{", i.Key);
                    js.AppendLine();

                    foreach (var j in i.Value)
                    {
                        js.AppendFormat("\t\t\"{0}\": 0x{1:X8}", j.Key, j.Value);
                        if (!i.Value.Last().Equals(j))
                            js.Append(",");
                        js.AppendLine();
                    }

                    js.Append("\t}");
                    if (!dict.Last().Equals(i))
                        js.Append(",");
                    js.AppendLine();
                }
                js.AppendLine("};");
                Console.Write(js);
                return;
            }

            Debug.Listeners.Add(new ConsoleTraceListener(false));
            var server = new WebsocketServer();
            server.RegisterManager(new ClientManager());
            server.RegisterManager(new ChannelManager());
            server.Start();
            Thread.Sleep(-1);
        }
    }
}