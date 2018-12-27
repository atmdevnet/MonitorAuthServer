﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using MonitorAuthServer.Model;
using System;

namespace MonitorAuthServer.Migrations
{
    [DbContext(typeof(MonitorAuthServer.Model.Database))]
    [Migration("20170920063736_scope")]
    partial class scope
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MonitorAuthServer.Model.AppVersion", b =>
                {
                    b.Property<DateTime>("ValidFrom");

                    b.Property<DateTime>("ValidTo");

                    b.Property<string>("RequiredAtLeast")
                        .IsRequired()
                        .HasMaxLength(24);

                    b.HasKey("ValidFrom", "ValidTo");

                    b.ToTable("AppVersion");
                });

            modelBuilder.Entity("MonitorAuthServer.Model.License", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<string>("Nick")
                        .IsRequired()
                        .HasMaxLength(16);

                    b.Property<string>("Note")
                        .HasMaxLength(128);

                    b.Property<int>("Scope");

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

                    b.Property<int>("Scope");

                    b.Property<long>("UserId");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.HasIndex("Date", "UserId", "ValidTo")
                        .IsUnique()
                        .HasFilter("[ValidTo] IS NOT NULL");

                    b.ToTable("LicenseAudit");
                });
#pragma warning restore 612, 618
        }
    }
}
