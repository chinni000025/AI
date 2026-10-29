using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class UpdateProjectSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Projects_ProjectId",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_Projects_ProjectId",
                table: "ProjectMembers");

            // 2. Drop BOTH indexes before their respective columns
            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_ProjectId",
                table: "ProjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_ProjectId",
                table: "Conversations");

            // 3. Replace Projects.Id (int IDENTITY) with GUID
            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Projects");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "Id");

            // 4. Replace ProjectMembers.ProjectId with GUID
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ProjectMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "ProjectMembers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            // Recreate ProjectMembers index
            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ProjectId",
                table: "ProjectMembers",
                column: "ProjectId");

            // 5. Replace Conversations.ProjectId with nullable GUID
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Conversations");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Conversations",
                type: "uniqueidentifier",
                nullable: true);

            // Recreate Conversations index
            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ProjectId",
                table: "Conversations",
                column: "ProjectId");

            // 6. Restore foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Projects_ProjectId",
                table: "Conversations",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_Projects_ProjectId",
                table: "ProjectMembers",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Projects",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "ProjectMembers",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Conversations",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
