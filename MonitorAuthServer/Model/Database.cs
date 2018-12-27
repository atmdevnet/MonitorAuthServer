using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MonitorAuthServer.Model
{
    public class Database : DbContext
    {
        public Database(DbContextOptions<Database> config)
            : base(config)
        {
        }

        public DbSet<License> Licenses { get; set; }
        public DbSet<LicenseAudit> Audit { get; set; }
        public DbSet<AppVersion> Version { get; set; }
        public DbSet<UserActivity> Activity { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<License>()
                .HasIndex(e => e.Nick)
                .IsUnique();

            modelBuilder.Entity<LicenseAudit>()
                .HasIndex(e => new { e.Date, e.UserId, e.ValidTo })
                .IsUnique();

            modelBuilder.Entity<AppVersion>()
                .HasKey(e => new { e.ValidFrom, e.ValidTo });

            modelBuilder.Entity<AppVersion>()
                .HasIndex(e => e.RequiredAtLeast)
                .IsUnique();

            modelBuilder.Entity<UserActivity>()
                .HasIndex(e => new { e.UserId, e.Date });
        }
    }


    [Table("License")]
    [DataContract]
    public class License
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Range(typeof(long), "1", "9223372036854775807")]
        [DataMember]
        public long UserId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(16, MinimumLength = 1)]
        [RegularExpression(@"^[a-zA-Z0-9_\-]{1,16}$")]
        [DataMember]
        public string Nick { get; set; }

        [DataMember]
        public DateTime? ValidTo { get; set; }

        [DataMember]
        public LicenseScope Scope { get; set; }

        [StringLength(128)]
        [DataMember]
        public string Note { get; set; }
    }


    [Table("LicenseAudit")]
    public class LicenseAudit
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public long UserId { get; set; }

        [StringLength(16)]
        public string Nick { get; set; }

        public DateTime? ValidTo { get; set; }

        public LicenseScope Scope { get; set; }

        [StringLength(128)]
        public string Note { get; set; }
    }


    [Table("AppVersion")]
    public class AppVersion
    {
        /// <summary>
        /// Key
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Key
        /// </summary>
        public DateTime ValidTo { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(24, MinimumLength = 7)]
        [RegularExpression(@"^\d{1,2}\.\d{1,3}\.\d{1,8}\.\d{1,8}$")]
        public string RequiredAtLeast { get; set; }
    }


    [Table("UserActivity")]
    public class UserActivity
    {
        [Key]
        public int Id { get; set; }

        public long UserId { get; set; }

        public DateTime Date { get; set; }

        [StringLength(24, MinimumLength = 7)]
        public string AppVersion { get; set; }
    }



    [Flags]
    public enum LicenseScope
    {
        Basic = 0,
        ShowInBrowser = 1,
        ExcludeSellers = 2,
        Buy = 4
    }



    /// <summary>
    /// helper for design time db creation
    /// </summary>
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<Database>
    {
        private string _cs = "Data Source=SQL5036.SmarterASP.NET;Initial Catalog=DB_9D811E_monitor;User Id=DB_9D811E_monitor_admin;Password=M1pcatm@1";
        //private string _cs = "Data Source=localhost\\sqlexpress;Initial Catalog=Monitor;Integrated Security=False;User Id=monitor;Password=monitor";

        public DesignTimeContextFactory()
        {
        }

        public Database CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<Database>();
            builder.UseSqlServer(_cs);
            return new Database(builder.Options);
        }
    }
}
