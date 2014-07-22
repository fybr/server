using Fybr.Server.Services;

namespace Fybr.Server
{
    static class Brain
    {
        public static SocketService Socket { get; set; }
        public static IDatabase Database { get; set; }
        public static IUserProvider Users { get; set; }

        static Brain()
        {
            Socket = new SocketService();
            Database = new CassandraService();
            Users = Database.BuildUserProvider();
        }


        public static void Force()
        {
            
        }
    }
}
