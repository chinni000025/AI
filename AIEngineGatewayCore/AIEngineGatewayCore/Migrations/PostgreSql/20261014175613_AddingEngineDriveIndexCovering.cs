using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddingEngineDriveIndexCovering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS
                    "IX_EngineFile_CreatedBy_Include_FileSize"
                ON public."EngineFiles" ("CreatedBy")
                INCLUDE ("FileSize");
                """, suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX CONCURRENTLY IF EXISTS
                    public."IX_EngineFile_CreatedBy_Include_FileSize";
                """, suppressTransaction: true);
        }
    }
}