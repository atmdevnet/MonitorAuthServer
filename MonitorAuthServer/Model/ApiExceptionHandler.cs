using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace MonitorAuthServer.Model
{
    public class ApiExceptionHandler
    {
        public static async Task Handler(HttpContext context)
        {
            if (!context.Response.HasStarted)
            {
                var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                var error = new AppError(exceptionHandler?.Error);

                context.Response.ContentType = "application/json";
                var body = Newtonsoft.Json.Linq.JObject.FromObject(error).ToString();

                await context.Response.WriteAsync(body);
            }
        }
    }
}
