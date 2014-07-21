using System;
using System.Collections.Generic;
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
                var user = await Brain.Users.Get(credentials);
                if (user == null)
                    return 401;
                return await Brain.Users.Session(user);
            };

            this.Post["users", true] = async (o,ct) =>
            {
                var credentials = this.Bind<UserRef>();
                var user = new UserRef(credentials);
                await Brain.Users.Save(user);
                var session = await Brain.Users.Session(user);
                return session;
            };


        }
    }
}
