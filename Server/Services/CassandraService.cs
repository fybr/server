using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CassandraSharp;
using CassandraSharp.Config;
using CassandraSharp.CQLPoco;
using Fybr.Server.Modules;
using Fybr.Server.Objects;

namespace Fybr.Server.Services
{
    public class CassandraService
    {
        private IPreparedQuery<NonQuery> _event;

        public CassandraService()
        {
            var config = new ClusterConfig
            {

                Name = "default",
                Endpoints = new EndpointsConfig()
                {
                    Servers = new[] { "127.0.0.1" }
                },

            };
            ClusterManager.Configure(new CassandraSharpConfig());
            Cluster = ClusterManager.GetCluster(config);

            Cluster.CreatePocoCommand().Execute("CREATE KEYSPACE IF NOT EXISTS fybr WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 1 };").AsFuture().Wait();

            Cluster.CreatePocoCommand().Execute("CREATE TABLE IF NOT EXISTS fybr.events ( " +
                                                "   user text, " +
                                                "   type text, " +
                                                "   id text, " +
                                                "   created timestamp," +
                                                "   data text," +
                                                "   PRIMARY KEY((user, type), id) " +
                                                ");").AsFuture().Wait();

            _event = Cluster.CreatePocoCommand().Prepare("UPDATE fybr.events SET " +
                                                         "  data = ?, " +
                                                         "  created = dateof(now()) " +
                                                         "WHERE" +
                                                         "  user = ? AND " +
                                                         "  type = ? AND " +
                                                         "  id = ?");

        }

        public ICluster Cluster { get; set; }

        public async Task Event(Event e)
        {
            await _event.Execute(e).AsFuture();
            //e.Device = "all";
            //await _event.Execute(e).AsFuture();
        }

        public async Task<IEnumerable<Event>> Get(UserRef user, string type)
        {
            return await Cluster.CreatePocoCommand().Prepare<Event>("SELECT * FROM fybr.events WHERE user = ? AND type = ?").Execute(new {user = user.User, type}).AsFuture();
        }
    }
}
