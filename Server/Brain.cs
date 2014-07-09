using Fybr.Server.Services;

namespace Fybr.Server
{
    static class Brain
    {
        public static SocketService Socket { get; set; }

        static Brain()
        {
            Socket = new SocketService();
        }
    }
}
