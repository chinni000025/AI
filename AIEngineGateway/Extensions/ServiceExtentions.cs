using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Helpers;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;
using AIEngineCore.Extensions;
using AIEngineCore.Services;
using AIEngineGateway.BackgroundServices;
using AIEngineGateway.BackgroundServices.Jobs;
using AIEngineGateway.Contracts;
using AIEngineGateway.EngineInfrastructure;
using AIEngineGateway.EngineInfrastructure.RateLimiter;
using AIEngineGateway.EngineInfrastructure.UserRateLimiter;
using AIEngineGateway.Helpers;
using AIEngineGateway.Repositories;
using AIEngineGateway.Services;
using AIEngineSpeechRecognition.Services;
using Quartz;
using Serilog;
using static AIEngineConnectivity.Constants.EngineConstants;

namespace AIEngineGateway.Extensions
{
    public static class ServiceExtentions
    {
        public static void AddEngineServices(this IServiceCollection services, IConfiguration config)
        {
            EngineDataBaseServices(services);
            EngineConfiguration(services);
            EngineRepositories(services);
            EngineServices(services);
            EncryptionExtensions(services);
            AIExtensions(services);
            EngineConfiguration(services, config);
            CleanUpJobs(services);
            CorsOrigin(services);
            Logger();
            AddEngineQuartzServices(services);
        }

        public static void EngineDataBaseServices(IServiceCollection services)
        {

            services.AddDbContext<SqlServerEngineContext>((ServiceProvider, Options) =>
             {
                 var configurator = ServiceProvider.GetRequiredService<EngineDbConfigurator>();
                 configurator.ConfigureEngineDataBase(ServiceProvider, Options, DataBaseProvider.SqlServer);
             });

            services.AddDbContext<PostgreSqlEngineContext>((ServiceProvider, Options) =>
            {
                var configurator = ServiceProvider.GetRequiredService<EngineDbConfigurator>();
                configurator.ConfigureEngineDataBase(ServiceProvider, Options, DataBaseProvider.PostgreSql);
            });

            services.AddScoped<EngineContext>(ServiceProvider =>
            {
                var engineConfig = ServiceProvider.GetRequiredService<EngineConfig>();
                if (!engineConfig.IsEngineConfig())
                {
                    return ServiceProvider.GetRequiredService<SqlServerEngineContext>();
                }

                return engineConfig.GetDatabaseType() switch
                {
                    DataBaseProvider.SqlServer => ServiceProvider.GetRequiredService<SqlServerEngineContext>(),
                    DataBaseProvider.PostgreSql => ServiceProvider.GetRequiredService<PostgreSqlEngineContext>(),
                    _ => throw new NotSupportedException($"The database provider '{engineConfig.GetDatabaseType()}' is not supported.")
                };
            });
        }

        public static void EngineConfiguration(IServiceCollection services)
        {
            services.AddSingleton<EngineConfig>();
            services.AddSingleton<DataBaseIntialiationServices>();
            services.AddSingleton<UserBucketCleanUpHostedService>();
            services.AddSingleton<StartUpMigrations>();
            services.AddSingleton<EngineState>();
            services.AddHttpContextAccessor();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSignalR();
            services.AddSingleton<UserBucketStore>();
            services.AddSingleton<UserRateLimiter>();
            services.AddSingleton<ServerRateLimiter>();
            services.AddControllers().AddNewtonsoftJson();
            services.AddScoped<IDataBaseProviderFactory, DataBaseProviderFactory>();
            services.AddScoped<PostgreServerProvider>();
            services.AddScoped<SqlServerProvider>();
            services.AddScoped<EngineDbConfigurator>();
            services.AddScoped<IEngineDataBaseService, EngineConfigureService>();
        }

        public static void EngineRepositories(IServiceCollection services)
        {
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            services.AddScoped(typeof(IEngineRepoBase<>), typeof(EngineRepoBase<>));
            services.AddScoped<IIdentityRepository, IdentityRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IConnectionRepository, ConnectionRepository>();
            services.AddScoped<IDataProtectionKeyRepository, DataProtectionKeyRepository>();
            services.AddScoped<IEngineNotificationRepository, EngineNotificationRepository>();
            services.AddScoped<IEngineNotificationEventService, EngineNotificationEventService>();
        }

        public static void EngineServices(IServiceCollection services)
        {
            services.AddScoped<IEngineStartUpService, EngineStartUpService>();
            services.AddScoped<IEngineConnectionService, EngineConnectionService>();
            services.AddHostedService<DataBaseCleanUpServices>();
            services.AddScoped<IIdentityHelper, IdentityHelpers>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddSingleton<IUserSessionManager, UserSessionManager>();
            services.AddSingleton<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordService, PasswordServices>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<IEngineScheduler, EngineScheduler>();
            services.AddScoped<IEngineNotificationService, EngineNotificationService>();
        }

        public static void AIExtensions(IServiceCollection services)
        {
            services.AddScoped<IAIOrchestrator, AIOrchestrator>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddSingleton<IWhisperService, WhisperService>();
            services.AddEngineCoreDependencies();
            services.AddSingleton<IEngineLatch, EngineLatch>();
        }

        public static void EngineConfiguration(IServiceCollection services, IConfiguration config)
        {
            services.Configure<SmtpConfiguration>(config.GetSection("Smtp"));
            services.Configure<JWTConfiguration>(config.GetSection("Jwt"));
            services.Configure<GeminiAPiKeyConfiguration>(config.GetSection("Gemini"));
            services.Configure<GroqApiKeyConfiguration>(config.GetSection("Groq"));
            services.Configure<HuggingFaceApiKeyConfiguration>(config.GetSection("HuggingFace"));
            services.Configure<OpenRouterAPiKeyConfiguration>(config.GetSection("OpenRouter"));
            services.Configure<CohereApiKeyConfiguration>(config.GetSection("Cohere"));
            services.Configure<WorkerConfiguration>(config.GetSection("WorkersConfiguration"));
            services.Configure<UserRateLimiterOptions>(config.GetSection("UserRateLimiter"));
            services.Configure<ServerRateLimiterOptions>(config.GetSection("ServerRateLimiter"));
        }

        public static void EncryptionExtensions(IServiceCollection services)
        {
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddSingleton<EnginePrivateKey>();
        }

        public static void CleanUpJobs(IServiceCollection services)
        {
            services.AddScoped<ICleanUpJob, RefreshTokenCleanUpJob>();
            services.AddScoped<ICleanUpJob, ResetTokenCleanUpJob>();
            services.AddScoped<ICleanUpJob, SoftDeleteCleanupJob>();
            services.AddScoped<ICleanUpJob, DeleteConversationsJob>();
        }

        public static void CorsOrigin(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();

                });
            });
        }

        public static void Logger()
        {
            Directory.CreateDirectory(EngineConstants.EngineLogDirectory);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: EngineConstants.EngineLogPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 5,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        public static void AddEngineQuartzServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var engineConfig = serviceProvider.GetRequiredService<EngineConfig>();
            services.AddQuartz(p =>
            {
                if (engineConfig.IsEngineConfig())
                {
                    var dbProvider = engineConfig.GetDatabaseType();
                    var connectionString = engineConfig.ConnectionString();
                    p.UsePersistentStore(store =>
                    {
                        store.UseProperties = true;
                        store.UseSystemTextJsonSerializer();
                        store.PerformSchemaValidation = false;
                        if (dbProvider == DataBaseProvider.SqlServer)
                        {
                            store.UseSqlServer(sql =>
                            {
                                sql.ConnectionString = connectionString;
                                sql.TablePrefix = "QRTZ_";
                            });
                        }
                        else if (dbProvider == DataBaseProvider.PostgreSql)
                        {
                            store.UsePostgres(postgres =>
                            {
                                postgres.ConnectionString = connectionString;
                                postgres.TablePrefix = "qrtz_";
                            });
                        }
                    });
                }
                else { return; }
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        }
    }
}
