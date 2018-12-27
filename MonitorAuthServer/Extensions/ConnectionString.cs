using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MonitorAuthServer.Extensions
{
    public class ConnectionString
    {
        public static string ForCurrentEnvironment(IConfiguration config)
        {
            var result = string.Empty;

            if (config != null)
            {
                var env = config["ASPNETCORE_ENVIRONMENT"];
                var cs = env?.Equals("Production") ?? false ? "Production" : "Development";

                result = config.GetConnectionString(cs);
            }

            return result;
        }
    }
}
