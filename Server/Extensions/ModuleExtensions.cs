using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace Fybr.Server.Extensions
{
    public static class ModuleExtensions
    {
        public static void PublicEndpoint(this NancyModule module)
        {
            module.After.AddItemToEndOfPipeline(ctx =>
            {
                ctx.Response.PublicEndpoint(ctx.Request);
            });
        }

        public static void PublicEndpoint(this Response response, Request request)
        {
            response
                      .WithHeader("Access-Control-Allow-Origin", request.Headers["origin"].FirstOrDefault() ?? "*")
                      .WithHeader("Access-Control-Allow-Methods", "POST, PUT, DELETE, GET")
                      .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type, X-Requested-With")
                      .WithHeader("Access-Control-Allow-Credentials", "true");

        }
    }
}
