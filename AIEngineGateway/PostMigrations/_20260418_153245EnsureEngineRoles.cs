using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineGateway.Contracts;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.PostMigrations
{
    public class _20260418_153245EnsureEngineRoles : IPostMigration
    {
        public string MigrationName => "_20260418_153245EnsureEngineRoles";

        public async Task ExecuteAsync(EngineContext engineContext)
        {
            var engineRoles = new List<EngineRole>
            {
                new EngineRole{Name = EngineRoles.User},
                new EngineRole{Name = EngineRoles.Assistant},
                new EngineRole{Name = EngineRoles.Owner},
                new EngineRole{Name = EngineRoles.Admin},
                new EngineRole{Name = EngineRoles.Member}
            };

            foreach (var role in engineRoles)
            {
                // needs to prevent existing things.
                var isExists = await engineContext.EngineRoles.AnyAsync(r => r.Name == role.Name);
                if (!isExists)
                {
                    await engineContext.EngineRoles.AddAsync(role);
                }
            }
        }
    }
}