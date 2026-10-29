using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEngineGateway.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddingEngineDriveIndexCovering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_EngineFile_CreatedBy_Include_FileSize'
                      AND object_id = OBJECT_ID(N'dbo.EngineFiles')
                )
                BEGIN
                    CREATE NONCLUSTERED INDEX
                        IX_EngineFile_CreatedBy_Include_FileSize
                    ON dbo.EngineFiles (CreatedBy)
                    INCLUDE (FileSize)
                    WITH (ONLINE = ON);
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_EngineFile_CreatedBy_Include_FileSize'
                      AND object_id = OBJECT_ID(N'dbo.EngineFiles')
                )
                BEGIN
                    DROP INDEX IX_EngineFile_CreatedBy_Include_FileSize
                    ON dbo.EngineFiles;
                END;
                """);
        }
    }
}