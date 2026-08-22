using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class UpdateEngineNotificationEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "EngineNotificationEvents");

            migrationBuilder.DropColumn(
                name: "IsRetriedEvent",
                table: "EngineNotificationEvents");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "EngineNotificationEvents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "EngineNotificationEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRetriedEvent",
                table: "EngineNotificationEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "EngineNotificationEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
