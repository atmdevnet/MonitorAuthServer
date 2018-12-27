using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MonitorAuthServer.Extensions
{
    public static class HttpSessionExtensions
    {
        public static void Store(this ISession session, byte[] value)
        {
            if (session?.IsAvailable ?? false)
            {
                session.Clear();
                session.Set(session.Id, value);
            }
        }

        public static byte[] Restore(this ISession session)
        {
            var result = new byte[0];

            if ((session?.IsAvailable ?? false) && session.TryGetValue(session.Id, out var value))
            {
                result = value;
            }

            return result;
        }

        public static void Store<T>(this ISession session, T value)
        {
            if (session?.IsAvailable ?? false)
            {
                session.Clear();
                session.Set<T>(session.Id, value);
            }
        }

        public static T Restore<T>(this ISession session)
        {
            var result = default(T);

            if (session?.IsAvailable ?? false)
            {
                result = session.Get<T>(session.Id);
            }

            return result;
        }

        public static void Set<T>(this ISession session, string key, T value)
        {
            if ((session?.IsAvailable ?? false) && !string.IsNullOrEmpty(key) && value != null)
            {
                var data = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(value));
                session.Set(key, data);
            }
        }

        public static T Get<T>(this ISession session, string key)
        {
            var result = default(T);

            if ((session?.IsAvailable ?? false) && !string.IsNullOrEmpty(key) && session.TryGetValue(key, out var data))
            {
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
            }

            return result;
        }
    }
}
