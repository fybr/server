using System.IO;
using System.Linq;
using System.Net;
using Fybr.Server.Extensions;
using Fybr.Server.Objects;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;

namespace Fybr.Server.Modules
{
    public class Event
    {
        public string User { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }


    public class UserModule : NancyModule
    {
        public UserModule()
            : base("users")
        {

            this.PublicEndpoint();

            var me = new UserRef();

            this.Before.AddItemToStartOfPipeline(context =>
            {
                string session = context.Request.Query.session;
                me.User = Brain.Users.Get(session).Result.User;
                return null;
            });

            this.Post["events/{type}/{id}", true] = async (o, ct) =>
            {
                var e = new Event
                {
                    User = me.User,
                    Type = o.type,
                    Data = new StreamReader(this.Request.Body).ReadToEnd()
                };
                await Brain.Cassandra.Event(e);
                Brain.Socket.Send(e);
                return Response.AsText("ok");
            };

            this.Get["events/{type}", true] = async (o,ct) =>
            {
                string type = o.type;
                var events = await Brain.Cassandra.Get(type);
                var json = "[" + string.Join(" , ", events.Select(ev => ev.Data)) + "]";
                return Response.AsText(json, "application/json");
            };

            this.Post["devices", true] = async (o, ct) =>
            {
                string id = new StreamReader(this.Request.Body).ReadToEnd();
                await Brain.Users.AddDevice(me, id);
                return Response.AsText("ok");
            };

            this.Post["devices/push", true] = async (o,ct) =>
            {
                var json = new StreamReader(this.Request.Body).ReadToEnd();
                var device = await Brain.Users.GetDevice(me);

                string data = "{ \"data\": " + json +
                              ", \"registration_ids\" : [\"" + device + "\"] }";
                var wc = new WebClient();
                wc.Headers.Add(HttpRequestHeader.Authorization, "key=AIzaSyB18lP-wiv0lkIDWlEim-JGskSpIBQPjaI");
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                try
                {
                    Response.AsText(wc.UploadString("https://android.googleapis.com/gcm/send", data));

                }
                catch (WebException e)
                {
                    return Response.AsText(new StreamReader(e.Response.GetResponseStream()).ReadToEnd());
                }
                return Response.AsOk();
            };
        }
    }
}
