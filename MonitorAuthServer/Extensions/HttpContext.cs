using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MonitorAuthServer.Extensions
{
    public static class HttpContextExtensions
    {
        public static bool? IsApiRequested(this HttpContext context)
        {
            return context?.Request.Path.StartsWithSegments(Extensions.Url.ApiBasePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
