using AIEngineBridge;
using Serilog;

class program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();
        builder.Services.InitializeEngineService(builder.Configuration);
        var app = builder.Build();
        app.InitializeEnginePipeline();
        await app.RunAsync();
    }
}