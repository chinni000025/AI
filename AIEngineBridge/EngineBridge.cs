using AIEngineConnectivity.Services;
using AIEngineCore.Extensions;
using AIEngineGateway.Extensions;
using AIEngineGateway.Hub;
using AIEngineGateway.Services;
using AIEngineRedis.Extensions;
using AIEngineSpeechRecognition.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AIEngineBridge
{
    public static class EngineBridge
    {
        public static IServiceCollection InitializeEngineService(this IServiceCollection service, IConfiguration config)
        {
            service.AddEngineServices(config);
            service.AddingJWtService(config);
            service.AddEngineCoreDependencies();
            service.AddEngineSpeechRecognition();
            service.AddControllersWithViews()
                .AddApplicationPart(typeof(AIEngineGateway.Controllers.ConversationController).Assembly)
                .AddNewtonsoftJson();
            service.AddEndpointsApiExplorer();
            service.AddSwaggerGen();
            service.AddOpenApi();
            service.AddAIEngineRedis(config);
            return service;
        }

        public static WebApplication InitializeEnginePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var resolvedWebRoot = app.Environment.WebRootPath;
            if (string.IsNullOrEmpty(resolvedWebRoot) || !Directory.Exists(resolvedWebRoot))
            {
                var baseDirWwwRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
                var devGatewayWwwRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "AIEngineGateway", "wwwroot"));

                if (Directory.Exists(baseDirWwwRoot))
                {
                    resolvedWebRoot = baseDirWwwRoot;
                }
                else if (Directory.Exists(devGatewayWwwRoot))
                {
                    resolvedWebRoot = devGatewayWwwRoot;
                }
            }

            if (!string.IsNullOrEmpty(resolvedWebRoot) && Directory.Exists(resolvedWebRoot))
            {
                var fileProvider = new PhysicalFileProvider(resolvedWebRoot);
                app.Environment.WebRootPath = resolvedWebRoot;
                app.Environment.WebRootFileProvider = fileProvider;

                app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
                app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
            }
            else
            {
                app.UseDefaultFiles();
                app.UseStaticFiles();
            }

            app.AddEngineMiddleware();

            var engineConfig = app.Services.GetRequiredService<IEngineConfigService>();
            if (engineConfig.IsEngineConfig())
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scoped = app.Services.CreateScope();
                        var initializeEngine = scoped.ServiceProvider.GetRequiredService<IEngineStartUpService>();
                        await initializeEngine.InitializeAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Fatal(ex, "Background engine initialization failed via AIEngineBridge.");
                    }
                });
            }

            app.MapHub<NotificationHub>("/api/notificationHub");
            app.MapHub<EngineStatusHub>("/api/engineStatusHub");
            app.MapHealthChecks("/health");
            app.MapControllers();

            app.MapFallback(async context =>
            {
                var path = context.Request.Path.Value ?? string.Empty;

                if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                    Path.HasExtension(path))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                context.Response.ContentType = "text/html";
                var indexPath = !string.IsNullOrEmpty(resolvedWebRoot)
                    ? Path.Combine(resolvedWebRoot, "index.html")
                    : Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html");

                if (File.Exists(indexPath))
                {
                    await context.Response.SendFileAsync(indexPath);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Frontend assets not found.");
                }
            });

            return app;
        }
    }
}