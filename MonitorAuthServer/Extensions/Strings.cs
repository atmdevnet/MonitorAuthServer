using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonitorAuthServer.Extensions
{
    public static class StringExtensions
    {
        public static bool IsBase64(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^[a-zA-Z0-9\+\/]+={0,2}$");
        }

        public static bool IsVersion(this string value)
        {
            return Version.TryParse(value, out var v);
        }

        public static Version ToVersion(this string value)
        {
            return Version.TryParse(value, out var v) ? v : default(Version);
        }
    }
}
