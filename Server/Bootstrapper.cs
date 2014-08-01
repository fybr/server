using System;
using System.Security.Cryptography.X509Certificates;
using Fybr.Server.Extensions;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.ErrorHandling;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Fybr.Server
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public Bootstrapper()
        {
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            StaticConfiguration.DisableErrorTraces = false;

            pipelines.AfterRequest += (ctx) =>
            {
                if (ctx.Request.Method == "OPTIONS")
                    ctx.Response.PublicEndpoint(ctx.Request);
            };

        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(typeof(JsonSerializer), typeof(CustomJsonSerializer));
        }
    }

    public class CustomJsonSerializer : JsonSerializer
    {
        public CustomJsonSerializer()
        {
            this.ContractResolver = new CamelCasePropertyNamesContractResolver();
            this.Formatting = Formatting.Indented;
        }
    }

    public class LoggingErrorHandler : IStatusCodeHandler 
    {

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.InternalServerError;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var exception = ((Exception)context.Items[NancyEngine.ERROR_EXCEPTION]).InnerException;
            Console.WriteLine(exception.ToString());
            Console.WriteLine(exception.StackTrace);
        }
    }
}
