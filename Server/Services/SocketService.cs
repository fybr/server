using System;
using System.Collections.Generic;
using Fleck;
using Fybr.Server.Modules;
using Fybr.Server.Objects;

namespace Fybr.Server.Services
{


    public class SocketService
    {
        private class Connection
        {
            public Connection(IWebSocketConnection connection)
            {
                User = GetUser(connection);
                Socket = connection;
            }

            public UserRef User { get; set; }
            public IWebSocketConnection Socket { get; set; }
        }

        private Dictionary<string, Connection> _subs;

        public SocketService()
        {
            _subs = new Dictionary<string, Connection>();
            var server = new WebSocketServer("ws://0.0.0.0:8181/hose");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    var c = new Connection(socket);
                    if(c.User == null) return;
                    Console.WriteLine(c.User.User + " connected");
                    _subs.Add(c.User.User, c);
                };
                socket.OnClose = () => _subs.Remove(GetUser(socket).User);
            });
        }

        private static UserRef GetUser(IWebSocketConnection connection)
        {
            var splits = connection.ConnectionInfo.Path.Split('/');
            return splits.Length < 3 ? null : Brain.Users.FromSession(splits[2]).Result;
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
