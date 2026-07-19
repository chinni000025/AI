# EF Core Migration Commands

Since we have separated the contexts into `SqlServerEngineContext` and `PostgreSqlEngineContext`, you must explicitly tell the Entity Framework Core CLI which context to use when adding or applying migrations.

> [!IMPORTANT]
> Make sure your terminal is opened in the project directory where the `csproj` file is located (e.g., `AIEngineGateway`).

## 1. Adding a New Migration

When you make changes to your entity models (like adding a new table or column), you need to generate a new migration for **both** databases separately.

### For SQL Server:
```bash
dotnet ef migrations add <MigrationName> -c SqlServerEngineContext -o Migrations/SqlServer
```
*(Replace `<MigrationName>` with a descriptive name, e.g., `AddUserTable`)*

### For PostgreSQL:
```bash
dotnet ef migrations add <MigrationName> -c PostgreSqlEngineContext -o Migrations/PostgreSql
```

---

## 2. Updating the Database (Applying Migrations)

If you need to manually apply the migrations to your development databases via the CLI (instead of letting the app apply them on startup), use the following commands:

### For SQL Server:
```bash
dotnet ef database update -c SqlServerEngineContext
```

### For PostgreSQL:
```bash
dotnet ef database update -c PostgreSqlEngineContext
```

---

## 3. Removing the Last Migration

If you made a mistake and haven't applied the migration to the database yet, you can remove the last generated migration:

### For SQL Server:
```bash
dotnet ef migrations remove -c SqlServerEngineContext
```

### For PostgreSQL:
```bash
dotnet ef migrations remove -c PostgreSqlEngineContext
```
