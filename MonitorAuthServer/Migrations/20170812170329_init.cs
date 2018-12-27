using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MonitorAuthServer.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "License",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    Nick = table.Column<string>(maxLength: 16, nullable: false),
                    Note = table.Column<string>(maxLength: 128, nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_License", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "LicenseAudit",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    Nick = table.Column<string>(maxLength: 16, nullable: true),
                    Note = table.Column<string>(maxLength: 128, nullable: true),
                    UserId = table.Column<long>(nullable: false),
                    ValidTo = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseAudit", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_License_Nick",
                table: "License",
                column: "Nick",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAudit_Date_UserId_ValidTo",
                table: "LicenseAudit",
                columns: new[] { "Date", "UserId", "ValidTo" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "License");

            migrationBuilder.DropTable(
                name: "LicenseAudit");
        }
    }
}
