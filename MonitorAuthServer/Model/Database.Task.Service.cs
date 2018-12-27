using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MonitorAuthServer.Model
{
    public class DatabaseTaskService
    {
        private DbContextOptions<Database> _dbo = null;


        public Exception LastError { get; private set; }

        public DatabaseTaskService(DbContextOptions<Database> dbo)
        {
            _dbo = dbo;
        }

        public void Run(Func<Database, Task> action)
        {
            Task.Run(async () =>
            {
                using (var db = new Database(_dbo))
                {
                    await action(db);
                }
            })
            .ContinueWith(t => {
                if (t.IsFaulted)
                {
                    LastError = t.Exception.GetBaseException();
                }
            });
        }
    }
}
