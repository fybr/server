using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CassandraSharp;
using CassandraSharp.Config;
using CassandraSharp.CQLPoco;
using Fybr.Server.Modules;

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
                                                "   created timestamp," +
                                                "   data text," +
                                                "   PRIMARY KEY((user, type), created) " +
                                                ");").AsFuture().Wait();

            _event = Cluster.CreatePocoCommand().Prepare("UPDATE fybr.events" +
                                                         "  SET data = ? " +
                                                         "WHERE" +
                                                         "  user = ? AND " +
                                                         "  type = ? AND " +
                                                         "  created = dateof(now());");

        }

        public ICluster Cluster { get; set; }

        public async Task Event(Event e)
        {
            await _event.Execute(e).AsFuture();
            //e.Device = "all";
            //await _event.Execute(e).AsFuture();
        }

        public async Task<IEnumerable<Event>> Get(string type)
        {
            return await Cluster.CreatePocoCommand().Execute<Event>("SELECT * FROM fybr.events").AsFuture();
        }
    }
}
