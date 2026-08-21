using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineGateway.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.EngineInfrastructure
{
#nullable disable
    public class PostMigrationRunner
    {
        private readonly EngineContext _engineContext;
        public PostMigrationRunner(EngineContext engineContext)
        {
            _engineContext = engineContext;
        }

        public async Task RunAsync()
        {
            var allMigrations = await DiscoverPostMigration();
            var appliedMigrationsName = await _engineContext.PostMigrations.Select(n => n.MigrationName).ToListAsync();
            var pendingMigrations = allMigrations.Where(m => !appliedMigrationsName.Contains(m.MigrationName)).ToList();

            foreach (var migrations in pendingMigrations)
            {
                using var transcation = await _engineContext.Database.BeginTransactionAsync();
                try
                {
                    await migrations.ExecuteAsync(_engineContext);
                    await _engineContext.PostMigrations.AddAsync(new PostMigration { MigrationName = migrations.MigrationName });
                    await _engineContext.SaveChangesAsync();
                    await transcation.CommitAsync();
                }
                catch
                {
                    await transcation.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<List<IPostMigration>> DiscoverPostMigration()
        {
            var migrationTypes = typeof(PostMigrationRunner).Assembly
                .GetTypes().Where(t => typeof(IPostMigration)
                .IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();

            var migrations = new List<IPostMigration>();
            foreach (var type in migrationTypes)
            {
                var instance = (IPostMigration)Activator.CreateInstance(type);
                migrations.Add(instance);
            }
            return migrations;
        }
    }
}