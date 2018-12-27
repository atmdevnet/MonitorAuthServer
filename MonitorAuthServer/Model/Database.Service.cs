using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitorAuthServer.Extensions;

namespace MonitorAuthServer.Model
{
    public class DatabaseService
    {
        private Database _context = null;
        private DatabaseTaskService _dbtask = null;

        public DatabaseService(Database context, DatabaseTaskService dbtask)
        {
            _context = context;
            _dbtask = dbtask;
        }

        public IEnumerable SelectActive()
        {
            var active = (from e in _context.Licenses
                          where e.ValidTo != null && e.ValidTo > DateTime.UtcNow
                          select e
                          ).ToList();

            foreach (var e in active)
            {
                e.ValidTo = e.ValidTo.RequireUtcFromStore();
            }

            return active;
        }

        public IEnumerable SelectExpired()
        {
            var expired = (from e in _context.Licenses
                          where e.ValidTo == null || e.ValidTo <= DateTime.UtcNow
                          select e
                          ).ToList();

            foreach (var e in expired)
            {
                e.ValidTo = e.ValidTo.RequireUtcFromStore();
            }

            return expired;
        }

        public bool Update(License license)
        {
            license.ValidTo = license.ValidTo.RequireUtcToStore();

            _context.Licenses.Update(license);
            return _context.SaveChanges() > 0;
        }

        public bool Remove(long id)
        {
            var license = _context.Licenses.Find(id);
            _context.Licenses.Remove(license);
            return _context.SaveChanges() > 0;
        }

        public bool Add(License license)
        {
            license.ValidTo = license.ValidTo.RequireUtcToStore();

            _context.Licenses.Add(license);
            return _context.SaveChanges() > 0;
        }

        public Version GetCurrentAppVersion()
        {
            var q = from v in _context.Version
                    where v.ValidFrom <= DateTime.UtcNow && DateTime.UtcNow < v.ValidTo
                    select v;

            return Version.TryParse(q.SingleOrDefault()?.RequiredAtLeast, out var version) ? version : default;
        }

        public long? UserExists(string nick)
        {
            var q = from l in _context.Licenses
                    where l.Nick.ToLower().Equals(nick.ToLower())
                    select l;

            return q.SingleOrDefault()?.UserId;
        }

        public License UserLicense(long userId)
        {
            var license = _context.Licenses.Find(userId);

            if (license != null)
            {
                license.ValidTo = license.ValidTo.RequireUtcFromStore();
            }

            return license;
        }

        public IEnumerable UserLicenseAudit(long userId)
        {
            var audit = (from a in _context.Audit
                     where a.UserId == userId
                     orderby a.Date descending
                     select a
                     ).ToList();

            foreach (var a in audit)
            {
                a.Date = a.Date.RequireUtcFromStore();
                a.ValidTo = a.ValidTo.RequireUtcFromStore();
            }

            return audit;
        }

        public IEnumerable UserActivity(long userId)
        {
            var activity = (from a in _context.Activity
                            where a.UserId == userId
                            orderby a.Date descending
                            select a)
                            .Take(10)
                            .ToList();

            foreach (var a in activity)
            {
                a.Date = a.Date.RequireUtcFromStore();
            }

            return activity;
        }

        public IEnumerable RecentActivity()
        {
            var activity = (from a in _context.Activity.Join(_context.Licenses, a => a.UserId, l => l.UserId, (a, l) => new { user = l.UserId, date = a.Date, version = a.AppVersion, nick = l.Nick })
                            orderby a.date descending
                            select a)
                            .Take(25)
                            .ToList();

            return activity.GroupBy(a => a.user)
                .Select(g => 
                {
                    var r = g.OrderByDescending(a => a.date).First();
                    return new { user = r.user, nick = r.nick, date = r.date.RequireUtcFromStore(), version = r.version };
                })
                .ToList();
        }

        public void SaveUserActivity(long userId, string version)
        {
            _dbtask.Run(async (db) =>
            {
                await db.Activity.AddAsync(new UserActivity() { UserId = userId, Date = DateTime.UtcNow.RequireUtcToStore(), AppVersion = version });
                await db.SaveChangesAsync();
            });
        }

        public IEnumerable SelectVersions()
        {
            var versions = (from v in _context.Version
                            orderby v.ValidTo descending
                            select v)
                            .ToList();

            foreach (var version in versions)
            {
                version.ValidFrom = version.ValidFrom.RequireUtcFromStore();
                version.ValidTo = version.ValidTo.RequireUtcFromStore();
            }

            return versions;
        }

        public async Task AppendVersion(AppVersion version)
        {
            version.ValidFrom = version.ValidFrom.RequireUtcToStore();
            version.ValidTo = DateTime.MaxValue.Date.RequireUtcToStore();

            var last = await _context.Version.OrderByDescending(v => v.ValidTo).FirstOrDefaultAsync();

            if (last != null)
            {
                if (last.ValidTo.RequireUtcFromStore().Date < DateTime.MaxValue.Date.RequireUtcToStore())
                {
                    throw new ArgumentException("Last version has invalid end date.");
                }

                if (version.ValidFrom <= last.ValidFrom.RequireUtcFromStore())
                {
                    throw new ArgumentException("Valid From date must be greater than current.");
                }

                if (version.RequiredAtLeast.ToVersion() <= last.RequiredAtLeast.ToVersion())
                {
                    throw new ArgumentException("Version must be greater than current.");
                }

                _context.Entry<AppVersion>(last).State = EntityState.Deleted;

                var modifiedLast = new AppVersion()
                {
                    ValidFrom = last.ValidFrom.RequireUtcToStore(),
                    ValidTo = version.ValidFrom,
                    RequiredAtLeast = last.RequiredAtLeast
                };

                await _context.Version.AddAsync(modifiedLast);
            }

            await _context.Version.AddAsync(version);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveVersion(AppVersion version)
        {
            var last = _context.Version.OrderByDescending(v => v.ValidTo).Take(2).ToList();

            if (!last?.Any() ?? true)
            {
                throw new ArgumentException("There is no entries to remove.");
            }

            var latest = last.First();

            if (latest.ValidFrom.RequireUtcFromStore() != version.ValidFrom.RequireUtcToStore() ||
                latest.ValidTo.RequireUtcFromStore() != version.ValidTo.RequireUtcToStore())
            {
                throw new ArgumentException("Only last entry can be removed.");
            }

            _context.Entry<AppVersion>(latest).State = EntityState.Deleted;

            if (last.Count == 2)
            {
                var previous = last.Last();

                _context.Entry<AppVersion>(previous).State = EntityState.Deleted;

                var modifiedPrevious = new AppVersion()
                {
                    ValidFrom = previous.ValidFrom.RequireUtcToStore(),
                    ValidTo = DateTime.MaxValue.Date.RequireUtcToStore(),
                    RequiredAtLeast = previous.RequiredAtLeast
                };

                await _context.Version.AddAsync(modifiedPrevious);
            }

            await _context.SaveChangesAsync();
        }


        //#region IDisposable Support
        //private bool disposedValue = false; // To detect redundant calls

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            // TODO: dispose managed state (managed objects).
        //            if (_context != null)
        //            {
        //                _context.Dispose();
        //            }
        //        }

        //        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        //        // TODO: set large fields to null.

        //        disposedValue = true;
        //    }
        //}

        //// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //// ~DatabaseService() {
        ////   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        ////   Dispose(false);
        //// }

        //// This code added to correctly implement the disposable pattern.
        //public void Dispose()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //    Dispose(true);
        //    // TODO: uncomment the following line if the finalizer is overridden above.
        //    // GC.SuppressFinalize(this);
        //}
        //#endregion
    }
}
