using AIEngineGateway.Contracts;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.BackgroundServices.Jobs
{
    public class RefreshTokenCleanUpJob : ICleanUpJob
    {

        public async Task ExecuteAsync(EngineContext engineContext)
        {

            // Time Buffer.
            var timeBuffer = DateTime.UtcNow.AddHours(-24);
            var expiredTokens = await engineContext.RefreshTokens
                .Where(t => t.ExpiresDate < timeBuffer && t.ExpiresDate < DateTime.UtcNow).Take(500)
                .ToListAsync();

            if (expiredTokens.Count > 0)
            {
                engineContext.RemoveRange(expiredTokens);
                await engineContext.SaveChangesAsync();
            }
        }
    }
}