using System;
using System.Collections.Generic;
using Fleck;

namespace Fybr.Server.Services
{
    public class SocketService
    {
        private HashSet<IWebSocketConnection> _subs; 

        public SocketService()
        {
            _subs = new HashSet<IWebSocketConnection>();
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () => _subs.Add(socket);
                socket.OnClose = () => Console.WriteLine("Close!");
            });
        }

        public void Send(string json, string token)
        {
            foreach (var ws in _subs)
            {
               ws.Send(json); 
            }
            
        }
    }
}
