using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CassandraSharp;
using CassandraSharp.CQLPoco;
using CassandraSharp.CQLPropertyBag;
using Fybr.Server.Objects;

namespace Fybr.Server.Services
{
    public class UserProvider
    {
        private readonly IPreparedQuery<NonQuery> _saveAuth;
        private readonly IPreparedQuery<NonQuery> _login;
        private readonly IPreparedQuery<UserRef> _fromAuth;
        private IPreparedQuery<UserRef> _fromSession;
        private IPreparedQuery<NonQuery> _addDevice;
        private IPreparedQuery<PropertyBag> _getDevice;

        public UserProvider()
        {
            Brain.Cassandra.Cluster.CreatePocoCommand().Execute("CREATE TABLE IF NOT EXISTS fybr.auth ( " +
                                                "   email text, " +
                                                "   password text, " +
                                                "   user text, " +
                                                "   PRIMARY KEY((email, password)) " +
                                                ");").AsFuture().Wait();

            Brain.Cassandra.Cluster.CreatePocoCommand().Execute("CREATE TABLE IF NOT EXISTS fybr.sessions ( " +
                                                "   id text, " +
                                                "   user text, " +
                                                "   PRIMARY KEY(id) " +
                                                ");").AsFuture().Wait();

            Brain.Cassandra.Cluster.CreatePocoCommand().Execute("CREATE TABLE IF NOT EXISTS fybr.devices ( " +
                                                "   device text, " +
                                                "   user text, " +
                                                "   PRIMARY KEY(user) " +
                                                ");").AsFuture().Wait();

            _saveAuth = Brain.Cassandra.Cluster.CreatePocoCommand().Prepare<NonQuery>(
                "UPDATE fybr.auth SET" +
                "   user = ? " +
                "WHERE " +
                "   email = ? AND " +
                "   password = ?");

            _fromAuth = Brain.Cassandra.Cluster.CreatePocoCommand().Prepare<UserRef>(
                "SELECT * FROM fybr.auth WHERE " +
                "   email = ? AND " +
                "   password = ?");

            _fromSession = Brain.Cassandra.Cluster.CreatePocoCommand().Prepare<UserRef>(
                "SELECT user FROM fybr.sessions WHERE id = ?");

            _login = Brain.Cassandra.Cluster.CreatePocoCommand().Prepare<NonQuery>("INSERT INTO fybr.sessions (id, user) VALUES (?,?) USING TTL 2592000");

            _addDevice = Brain.Cassandra.Cluster.CreatePocoCommand().Prepare<NonQuery>("UPDATE fybr.devices SET device = ? WHERE user = ?");

            _getDevice = Brain.Cassandra.Cluster.CreatePropertyBagCommand().Prepare("SELECT device FROM fybr.devices WHERE user = ?");
        }

        public async Task Save(UserRef user)
        {
            await _saveAuth.Execute(user).AsFuture();
        }

        public async Task<UserRef> Get(Credentials credentials)
        {
            return (await _fromAuth.Execute(credentials).AsFuture()).FirstOrDefault();
        }

        public async Task<UserRef> Get(string session)
        {
            return (await _fromSession.Execute(new { id = session }).AsFuture()).FirstOrDefault();
        }

        public async Task<string> GetDevice(UserRef me)
        {
            var bag = new PropertyBag();
            bag["user"] = me.User;
            var f = (await _getDevice.Execute(bag).AsFuture()).FirstOrDefault();
            if (f == null)
                return null;
            return (string) f["device"];
        }

        public async Task AddDevice(UserRef user, string device)
        {
            await _addDevice.Execute(new {user = user.User, device = device}).AsFuture();
        }

        public async Task<string> Session(UserRef user)
        {
            var id = Guid.NewGuid().ToString();
            await _login.Execute(new {user = user.User, id = id}).AsFuture();
            return id;
        }
    }
}
