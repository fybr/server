using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace Fybr.Server.Extensions
{
    static class ResponseExtensions
    {
        public static Response AsOk(this IResponseFormatter response)
        {
            return response.AsText("ok");
        }

        public static Response AsError(this IResponseFormatter response, string message, int code = 500)
        {
            return response.AsJson(message).WithStatusCode(code);
        }
    }
}
