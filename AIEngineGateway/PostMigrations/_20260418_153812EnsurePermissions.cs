namespace AIEngineGateway.PostMigrations
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Models;
    using AIEngineGateway.Contracts;
    using AIEngineGateway.EngineInfrastructure;
    using Microsoft.EntityFrameworkCore;

    public class _20260418_153812EnsurePermissions : IPostMigration
    {
        public string MigrationName => "_20260418_153812EnsurePermissions";

        public async Task ExecuteAsync(EngineContext engineContext)
        {
            var permissions = new List<Permission>
            {
                new Permission{Name =Permissions.Read},
                new Permission{Name =Permissions.Write}
            };

            foreach (var perm in permissions)
            {
                var isExists = await engineContext.Permissions.AnyAsync(p => p.Name == perm.Name);
                if (!isExists)
                {
                    await engineContext.Permissions.AddAsync(perm);
                }
            }
        }
    }
}