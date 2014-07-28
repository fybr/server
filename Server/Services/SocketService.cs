using System;
using System.Collections.Generic;
using Fleck;
using Fybr.Server.Modules;

namespace Fybr.Server.Services
{


    public class SocketService
    {
        private class Connection
        {
            public Connection(IWebSocketConnection connection)
            {
                User = connection.ConnectionInfo.Path.Split('/')[1];
                Socket = connection;
            }

            public string User { get; set; }
            public IWebSocketConnection Socket { get; set; }
        }

        private Dictionary<string, Connection> _subs;

        public SocketService()
        {
            _subs = new Dictionary<string, Connection>();
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    var c = new Connection(socket);
                    Console.WriteLine(c.User + " connected");
                    _subs.Add(Guid.NewGuid().ToString(), c);
                };
                socket.OnClose = () => _subs.Remove(new Connection(socket).User);
            });
        }

        public void Send(Event e)
        {
            Connection c = null;
            if(!_subs.TryGetValue(e.User, out c))
                return;

            c.Socket.Send(e.Data);
        }
    }
}
