using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MonitorAuthServer.Model;

namespace MonitorAuthServer.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20170812170329_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MonitorAuthServer.Model.License", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<string>("Nick")
                        .IsRequired()
                        .HasMaxLength(16);

                    b.Property<string>("Note")
                        .HasMaxLength(128);

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("UserId");

                    b.HasIndex("Nick")
                        .IsUnique();

                    b.ToTable("License");
                });

            modelBuilder.Entity("MonitorAuthServer.Model.LicenseAudit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("Nick")
                        .HasMaxLength(16);

                    b.Property<string>("Note")
                        .HasMaxLength(128);

                    b.Property<long>("UserId");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.HasIndex("Date", "UserId", "ValidTo")
                        .IsUnique();

                    b.ToTable("LicenseAudit");
                });
        }
    }
}
