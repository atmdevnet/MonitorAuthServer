using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MonitorAuthServer.Model
{
    public class AuthorityConfig
    {
        public string Current { get; set; }
        public IEnumerable<AuthorityConfigItem> Authorities { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Current) && (Authorities?.Any(a => a.Name.Equals(Current, StringComparison.OrdinalIgnoreCase)) ?? false);
        }

        public AuthorityConfigItem GetCurrent()
        {
            return Authorities?.FirstOrDefault(a => a.Name.Equals(Current, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class AuthorityConfigItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string ApiId { get; set; }
    }



    
}
