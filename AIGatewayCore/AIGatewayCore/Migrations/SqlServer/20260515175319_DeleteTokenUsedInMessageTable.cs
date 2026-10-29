using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class DeleteTokenUsedInMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokensUsed",
                table: "Messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokensUsed",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
