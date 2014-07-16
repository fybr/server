using Fybr.Server.Services;

namespace Fybr.Server
{
    static class Brain
    {
        public static SocketService Socket { get; set; }
        public static CassandraService Cassandra{ get; set; }

        static Brain()
        {
            Socket = new SocketService();
            Cassandra = new CassandraService();
        }

        public static void Force()
        {
            
        }
    }
}
