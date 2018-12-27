using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MonitorAuthServer.Extensions
{
    public static class Url
    {
        public static PathString ApiBasePath { get; private set; }
        public static PathString SilentBasePath { get; private set; }

        static Url()
        {
            ApiBasePath = PathString.FromUriComponent(new System.Uri("http://localhost/api", UriKind.Absolute));
            SilentBasePath = PathString.FromUriComponent(new System.Uri("http://localhost/silent", UriKind.Absolute));
        }
    }
}
