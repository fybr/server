using System;
using System.Collections.Generic;
using System.Linq;
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

        private MultiDictionary<string, Connection> _subs;

        public SocketService()
        {
            _subs = new MultiDictionary<string, Connection>();
            var server = new WebSocketServer("ws://0.0.0.0:8181/hose");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    var c = new Connection(socket);
                    if (c.User == null) return;
                    Console.WriteLine(c.User.User + " connected");
                    _subs.Add(c.User.User, c);
                };
                socket.OnClose = () =>
                {
                    var user = GetUser(socket);
                    var conns = _subs.Get(user.User);
                    var target = conns.FirstOrDefault(connection => connection.Socket == socket);
                    if(target == null) return;
                    conns.Remove(target);
                };
            });
        }

        private static UserRef GetUser(IWebSocketConnection connection)
        {
            var splits = connection.ConnectionInfo.Path.Split('/');
            return splits.Length < 3 ? null : Brain.Users.FromSession(splits[2]).Result;
        }

        public void Send(Event e)
        {
            var connections = _subs.Get(e.User);
            if(connections == null) return;
            foreach (var c in connections)
            {
                c.Socket.Send(e.Data);
            }
        }
    }
}
