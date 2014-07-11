using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Fybr.Server.Extensions;
using Nancy;

namespace Fybr.Server.Modules
{
    public class PushModule : NancyModule
    {
        public PushModule()
            : base("push")
        {
            this.PublicEndpoint();

            this.Post[""] = o =>
            {
                var json = new StreamReader(this.Request.Body).ReadToEnd();

                string data = "{ \"data\": " + json +
                              ", \"registration_ids\" : [\" APA91bGj-GeVWkSa7-jzz4wj6CbZEyzqEqNebLUeQb2spv_8YDSCRcE3OTfhK4ozjZLP7pvMRhj8ECc7j4sp69vl6YoFbjoaP_UXVV7okkp0stnZUfXKhxmhCkpzPXylWfn3QlLEnIS6XgVd5umqH0M25oOf04ALsA \"] }";
                var wc = new WebClient();
                wc.Headers.Add(HttpRequestHeader.Authorization, "key=AIzaSyB18lP-wiv0lkIDWlEim-JGskSpIBQPjaI");
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                try
                {
                    wc.UploadData("https://android.googleapis.com/gcm/send", Encoding.UTF8.GetBytes(data));
                }
                catch (WebException e)
                {
                    return Response.AsText(new StreamReader(e.Response.GetResponseStream()).ReadToEnd());
                }
                return 200;
            };
        }


    }
}
