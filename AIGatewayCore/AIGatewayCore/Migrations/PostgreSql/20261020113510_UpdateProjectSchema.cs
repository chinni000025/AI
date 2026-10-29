using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.PostgreSql
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

            // 2. Drop indexes before dropping columns
            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_ProjectId",
                table: "ProjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_ProjectId",
                table: "Conversations");

            // 3. Drop legacy ProjectId column on Projects if present
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Projects");

            // 4. Replace Projects.Id (integer) with UUID
            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Projects");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Projects",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "Id");

            // 5. Replace ProjectMembers.ProjectId with UUID
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ProjectMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "ProjectMembers",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ProjectId",
                table: "ProjectMembers",
                column: "ProjectId");

            // 6. Replace Conversations.ProjectId with nullable UUID
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Conversations");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ProjectId",
                table: "Conversations",
                column: "ProjectId");

            // 7. Re-add foreign keys
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
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Projects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "ProjectMembers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Conversations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
