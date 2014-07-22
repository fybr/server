﻿using System;
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
    public interface IUserProvider
    {
        Task<UserRef> Get(Credentials credentials);
        Task<UserRef> Get(string session);
        Task Save(UserRef user);
        Task<string> GetDevice(UserRef me);
        Task AddDevice(UserRef user, string device);
        Task<string> Session(UserRef user);
    }

    public class CassandraUserProvider : IUserProvider
    {
        private readonly IPreparedQuery<NonQuery> _saveAuth;
        private readonly IPreparedQuery<NonQuery> _login;
        private readonly IPreparedQuery<UserRef> _fromAuth;
        private IPreparedQuery<UserRef> _fromSession;
        private IPreparedQuery<NonQuery> _addDevice;
        private IPreparedQuery<PropertyBag> _getDevice;

        public CassandraUserProvider(ICluster cluster)
        {
            cluster.CreatePocoCommand().Execute("CREATE TABLE IF NOT EXISTS fybr.auth ( " +
                                                "   email text, " +
                                                "   password text, " +
                                                "   user text, " +
                                                "   PRIMARY KEY((email, password)) " +
                                                ");").AsFuture().Wait();

            cluster.CreatePocoCommand().Execute("CREATE TABLE IF NOT EXISTS fybr.sessions ( " +
                                                "   id text, " +
                                                "   user text, " +
                                                "   PRIMARY KEY(id) " +
                                                ");").AsFuture().Wait();

            cluster.CreatePocoCommand().Execute("CREATE TABLE IF NOT EXISTS fybr.devices ( " +
                                                "   device text, " +
                                                "   user text, " +
                                                "   PRIMARY KEY(user) " +
                                                ");").AsFuture().Wait();

            _saveAuth = cluster.CreatePocoCommand().Prepare<NonQuery>(
                "UPDATE fybr.auth SET" +
                "   user = ? " +
                "WHERE " +
                "   email = ? AND " +
                "   password = ?");

            _fromAuth = cluster.CreatePocoCommand().Prepare<UserRef>(
                "SELECT * FROM fybr.auth WHERE " +
                "   email = ? AND " +
                "   password = ?");

            _fromSession = cluster.CreatePocoCommand().Prepare<UserRef>(
                "SELECT user FROM fybr.sessions WHERE id = ?");

            _login = cluster.CreatePocoCommand().Prepare<NonQuery>("INSERT INTO fybr.sessions (id, user) VALUES (?,?) USING TTL 2592000");

            _addDevice = cluster.CreatePocoCommand().Prepare<NonQuery>("UPDATE fybr.devices SET device = ? WHERE user = ?");

            _getDevice = cluster.CreatePropertyBagCommand().Prepare("SELECT device FROM fybr.devices WHERE user = ?");
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