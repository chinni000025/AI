using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class EngineConnectionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "EngineConnections",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsConnected",
                table: "EngineConnections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "EngineConnections",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "EngineConnections");

            migrationBuilder.DropColumn(
                name: "IsConnected",
                table: "EngineConnections");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "EngineConnections");
        }
    }
}
