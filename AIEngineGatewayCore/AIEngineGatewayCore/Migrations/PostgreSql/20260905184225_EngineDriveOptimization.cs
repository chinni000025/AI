using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class EngineDriveOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileChunks_SessionId",
                table: "FileChunks");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "FileContents",
                newName: "ContentData");

            migrationBuilder.RenameColumn(
                name: "Chunk",
                table: "FileChunks",
                newName: "ChunkData");

            migrationBuilder.AddColumn<long>(
                name: "ContentOid",
                table: "FileContents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ChunkOid",
                table: "FileChunks",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileChunks_SessionId_ChunkIndex",
                table: "FileChunks",
                columns: new[] { "SessionId", "ChunkIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileChunks_SessionId_ChunkIndex",
                table: "FileChunks");

            migrationBuilder.DropColumn(
                name: "ContentOid",
                table: "FileContents");

            migrationBuilder.DropColumn(
                name: "ChunkOid",
                table: "FileChunks");

            migrationBuilder.RenameColumn(
                name: "ContentData",
                table: "FileContents",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "ChunkData",
                table: "FileChunks",
                newName: "Chunk");

            migrationBuilder.CreateIndex(
                name: "IX_FileChunks_SessionId",
                table: "FileChunks",
                column: "SessionId");
        }
    }
}
