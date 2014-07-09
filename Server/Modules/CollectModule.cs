using System.IO;
using Nancy;

namespace Fybr.Server.Modules
{
    public class CollectModule : NancyModule
    {
        public CollectModule()
        {
            this.Post["collect/{token}"] = o =>
            {
                string token = o.token;
                var json = new StreamReader(this.Request.Body).ReadToEnd();
                //var model = JsonConvert.DeserializeObject(json);
                //Console.WriteLine(model);
                Brain.Socket.Send(json, token);

                return Response.AsText("ok");
            };
        }
    }
}
