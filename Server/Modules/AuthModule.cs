using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fybr.Server.Extensions;
using Fybr.Server.Objects;
using Nancy;
using Nancy.ModelBinding;

namespace Fybr.Server.Modules
{
    public class AuthModule : NancyModule
    {
        public AuthModule()
        {
            this.PublicEndpoint();

            this.Post["users/login", true] = async (o,ct) =>
            {
                var credentials = this.Bind<Credentials>();
                credentials.Email = credentials.Email.ToLower();
                var user = await Brain.Users.FromEmail(credentials.Email);
                if(user == null || user.Password != credentials.Password)
                    return 401;
                return await Brain.Users.CreateSession(user);
            };

            this.Post["users", true] = async (o,ct) =>
            {
                var credentials = this.Bind<Credentials>();
                credentials.Email = credentials.Email.ToLower();
                var user = await Brain.Users.FromEmail(credentials.Email);
                if (user != null)
                    return 401;
                user = new UserRef(credentials);
                await Brain.Users.Save(user);
                var session = await Brain.Users.CreateSession(user);
                return session;
            };


        }
    }
}
