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
                              ", \"registration_ids\" : [\"APA91bEPWu1Jf0uBONAWbpexj7EpF1u_r8RHF_qbEC-V6Wtftep5Yr1CoMTyIOed97wKktu5YHrYBfa2gjSy20NA_LGWbQ0iov0GBZt9cHcTYIkHbFI6bW4hmUaIzl8p5G8_0jDwKF__nmnyab9rq_V_VqpKGX8ELg\"] }";
                var wc = new WebClient();
                wc.Headers.Add(HttpRequestHeader.Authorization, "key=AIzaSyB18lP-wiv0lkIDWlEim-JGskSpIBQPjaI");
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                try
                {
                    return Response.AsText(wc.UploadString("https://android.googleapis.com/gcm/send", data));
                }
                catch (WebException e)
                {
                    return Response.AsText(new StreamReader(e.Response.GetResponseStream()).ReadToEnd());
                }
            };
        }


    }
}
