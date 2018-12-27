using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MonitorAuthServer.Migrations
{
    public partial class version_index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppVersion_RequiredAtLeast",
                table: "AppVersion",
                column: "RequiredAtLeast",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppVersion_RequiredAtLeast",
                table: "AppVersion");
        }
    }
}
