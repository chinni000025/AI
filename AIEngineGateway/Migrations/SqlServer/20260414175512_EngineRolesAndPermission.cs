using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class EngineRolesAndPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EngineRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngineRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_RoleId",
                table: "ProjectMembers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RoleId",
                table: "Messages",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessages_RoleId",
                table: "GroupMessages",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupChatMember_RoleId",
                table: "GroupChatMember",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationShares_PermissionId",
                table: "ConversationShares",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationShares_Permissions_PermissionId",
                table: "ConversationShares",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupChatMember_EngineRoles_RoleId",
                table: "GroupChatMember",
                column: "RoleId",
                principalTable: "EngineRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_EngineRoles_RoleId",
                table: "GroupMessages",
                column: "RoleId",
                principalTable: "EngineRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_EngineRoles_RoleId",
                table: "Messages",
                column: "RoleId",
                principalTable: "EngineRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_EngineRoles_RoleId",
                table: "ProjectMembers",
                column: "RoleId",
                principalTable: "EngineRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationShares_Permissions_PermissionId",
                table: "ConversationShares");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupChatMember_EngineRoles_RoleId",
                table: "GroupChatMember");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_EngineRoles_RoleId",
                table: "GroupMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_EngineRoles_RoleId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_EngineRoles_RoleId",
                table: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "EngineRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_RoleId",
                table: "ProjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_Messages_RoleId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_GroupMessages_RoleId",
                table: "GroupMessages");

            migrationBuilder.DropIndex(
                name: "IX_GroupChatMember_RoleId",
                table: "GroupChatMember");

            migrationBuilder.DropIndex(
                name: "IX_ConversationShares_PermissionId",
                table: "ConversationShares");
        }
    }
}
