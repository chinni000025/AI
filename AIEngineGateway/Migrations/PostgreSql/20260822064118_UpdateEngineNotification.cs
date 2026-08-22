using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UpdateEngineNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationPriority",
                table: "EngineNotifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationPriority",
                table: "EngineNotifications");
        }
    }
}
