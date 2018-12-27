using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorAuthServer.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime? RequireUtcFromStore(this DateTime? value)
        {
            DateTime? result = null;

            if (value.HasValue)
            {
                if (value.Value.Kind == DateTimeKind.Unspecified)
                {
                    result = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                }
                else if (value.Value.Kind == DateTimeKind.Local)
                {
                    throw new Exception("Database datetime field value has wrong local kind. ");
                }
                else
                {
                    result = value;
                }
            }

            return result;
        }

        public static DateTime RequireUtcFromStore(this DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
            else if (value.Kind == DateTimeKind.Local)
            {
                throw new Exception("Database datetime field value has wrong local kind. ");
            }
            else
            {
                return value;
            }
        }

        public static DateTime? RequireUtcToStore(this DateTime? value)
        {
            DateTime? result = null;

            if (value.HasValue)
            {
                if (value.Value.Kind == DateTimeKind.Unspecified)
                {
                    result = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                }
                else if (value.Value.Kind == DateTimeKind.Local)
                {
                    result = value.Value.ToUniversalTime();
                }
                else
                {
                    result = value;
                }
            }

            return result;
        }

        public static DateTime RequireUtcToStore(this DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
            else if (value.Kind == DateTimeKind.Local)
            {
                return value.ToUniversalTime();
            }
            else
            {
                return value;
            }
        }
    }
}
