using AIEngineConnectivity.Services;
using AIEngineGateway.EngineInfrastructure;
using AIEngineGateway.Extensions;
using AIEngineGateway.Hub;
using AIEngineGateway.Services;
using Serilog;
class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEngineServices(builder.Configuration);
        builder.Services.AddingJWtService(builder.Configuration);
        builder.Services.AddControllersWithViews();
        builder.Services.AddOpenApi();
        builder.Host.UseSerilog();

        var app = builder.Build();
        app.MapHub<NotificationHub>("/api/notificationHub");
        app.MapHub<EngineStatusHub>("/api/engineStatusHub");
        app.AddEngineMiddleware();
        var engineConfig = app.Services.GetRequiredService<EngineConfig>();
        if (engineConfig.IsEngineConfig())
        {
            _ = Task.Run(async () =>
            {
                using var scoped = app.Services.CreateScope();
                var initializeEngine = scoped.ServiceProvider.GetRequiredService<IEngineStartUpService>();
                await initializeEngine.InitializeAsync();
            });
        }
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapControllers();
        app.MapFallback(async context =>
        {
            var path = context.Request.Path.Value;

            if (path != null &&
                (path.StartsWith("/api") || path.StartsWith("/swagger")))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
        });
        app.Run();
    }
}
