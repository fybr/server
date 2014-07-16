using System.IO;
using System.Linq;
using Fybr.Server.Extensions;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;

namespace Fybr.Server.Modules
{
    public class Event
    {
        public string Device { get; set; }
        public string User { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }


    public class EventModule : NancyModule
    {
        public EventModule()
            : base("users/{user}/devices/{device}/events")
        {

            this.PublicEndpoint();

            var e = new Event();

            this.Before.AddItemToStartOfPipeline(context =>
            {
                e.User = context.Parameters.user;
                e.Device = context.Parameters.device;
                return null;
            });

            this.Post["{type}", true] = async (o, ct) =>
            {
                e.Type = o.type;
                e.Data = new StreamReader(this.Request.Body).ReadToEnd();
                await Brain.Cassandra.Event(e);
                Brain.Socket.Send(e);
                return Response.AsText("ok");
            };

            this.Get["{type}", true] = async (o,ct) =>
            {
                string type = o.type;
                var events = await Brain.Cassandra.Get(type);
                var json = "[" + string.Join(" , ", events.Select(ev => ev.Data)) + "]";
                return Response.AsText(json, "application/json");
            };
        }
    }
}
