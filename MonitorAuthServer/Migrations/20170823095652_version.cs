using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MonitorAuthServer.Migrations
{
    public partial class version : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LicenseAudit_Date_UserId_ValidTo",
                table: "LicenseAudit");

            migrationBuilder.CreateTable(
                name: "AppVersion",
                columns: table => new
                {
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiredAtLeast = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVersion", x => new { x.ValidFrom, x.ValidTo });
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAudit_Date_UserId_ValidTo",
                table: "LicenseAudit",
                columns: new[] { "Date", "UserId", "ValidTo" },
                unique: true,
                filter: "[ValidTo] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppVersion");

            migrationBuilder.DropIndex(
                name: "IX_LicenseAudit_Date_UserId_ValidTo",
                table: "LicenseAudit");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAudit_Date_UserId_ValidTo",
                table: "LicenseAudit",
                columns: new[] { "Date", "UserId", "ValidTo" },
                unique: true);
        }
    }
}
