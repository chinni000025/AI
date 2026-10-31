# Connectivity-Centered Dependency Inversion Architecture Guide
## Segregation of AIEngineGateway, AIGatewayCore, AIEngineCore, & AIEngineSpeechRecognition

---

## 1. Architectural Blueprint: The Connectivity-Centered Inversion Design

In this architecture, **`AIEngineConnectivity` acts as the single central contract hub (Ports & Adapters / Onion Architecture)**. 

### Core Design Rules
1. **Zero Peer Couplings**: Service modules (`AIGatewayCore`, `AIEngineCore`, `AIEngineSpeechRecognition`) **never** reference each other directly. They strictly depend on `AIEngineConnectivity`.
2. **Gateway Host Inversion**: `AIEngineGateway` is purely the ASP.NET Core Web Host (Composition Root & HTTP presentation). Its Controllers and Middlewares **only communicate with services via interfaces defined in `AIEngineConnectivity`**.
3. **Core-to-Core Inversion**: `AIGatewayCore` communicates with `AIEngineCore` (AI LLM orchestration, pub/sub notifications) and `AIEngineSpeechRecognition` (Whisper STT/TTS) **strictly through `AIEngineConnectivity` interfaces** (`IAIOrchestrator`, `IEngineEventPublisher`, `IEngineBus`, `IWhisperService`).
4. **Composition Root**: `AIEngineGateway\Program.cs` is the only place where concrete modules are wired into the Microsoft DI container (`IServiceCollection`).

```
                              ┌────────────────────────────────────────┐
                              │          AIEngineConnectivity          │
                              │ ────────────────────────────────────── │
                              │  • Service & Repository Interfaces     │
                              │  • Core DTOs, Entities, Domain Models  │
                              │  • Event & Notification Contracts      │
                              │  • System Constants                    │
                              └───────▲──────────▲───────────▲─────────┘
                                      │          │           │
                 ┌────────────────────┴┐         │           └────────────────────┐
                 │ depends on          │ depends │                                │ depends on
                 │ interfaces          │ on      │                                │ interfaces
                 │                     │         │                                │
      ┌──────────────────────┐         │         │                     ┌──────────────────────┐
      │    AIGatewayCore     │         │         │                     │AIEngineSpeechRecogn. │
      │ ──────────────────── │         │         │                     │ ──────────────────── │
      │ • DbContexts & Repos │         │         │                     │ • Whisper STT        │
      │ • Domain Services    │         │         │                     │ • FFmpeg Audio       │
      │ • Background Jobs    │         │         │                     │ • Implements:        │
      │ • SignalR Hubs       │         │         │                     │   - IWhisperService  │
      │ • Implements:        │         │         │                     └──────────▲───────────┘
      │   - IConversationSvc │         │         │                                │
      │   - IIdentityService │         │         │                                │
      │   - IEngineDriveSvc  │         │         │                                │
      │   - IRepositoryWrap. │         │         │                                │
      │   - IUserRateLimiter │         │         │                                │
      │   - IServerRateLimiter         │         │                                │
      └──────────▲───────────┘         │         │                                │
                 │                     │         │                                │
                 │             ┌───────┴─────────┴──────┐                         │
                 │             │      AIEngineCore      │                         │
                 │             │ ────────────────────── │                         │
                 │             │ • AI Provider Routers  │                         │
                 │             │ • Pub/Sub Workers      │                         │
                 │             │ • Template Engine      │                         │
                 │             │ • Implements:          │                         │
                 │             │   - IAIOrchestrator    │                         │
                 │             │   - IEngineBus         │                         │
                 │             │   - IEngineEventPubl.  │                         │
                 │             └───────────▲────────────┘                         │
                 │                         │                                      │
                 └─────────────────────────┼──────────────────────────────────────┘
                                           │  Wired via DI at startup
                                           │  (Composition Root)
                                ┌──────────────────────┐
                                │   AIEngineGateway    │
                                │ ──────────────────── │
                                │ • Web API Host       │
                                │ • Controllers        │
                                │ • Middlewares        │
                                │ • Static Angular SPA │
                                │ • Program.cs (DI)    │
                                └──────────────────────┘
```

---

## 2. Inventory of Implemented Contracts in `AIEngineConnectivity`

### 2.1. `IEngineEventPublisher.cs` (Decouples `AIGatewayCore` from `AIEngineCore`)
* **Location**: `AIEngineConnectivity\EngineCore\IEngineEventPublisher.cs`
* **Implemented By**: `AIEngineCore.EngineCore.EngineEventPublisher`
* **Consumed By**: `AIGatewayCore.Services.IdentityService`
```csharp
using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Models;

namespace AIEngineConnectivity.EngineCore
{
    public interface IEngineEventPublisher
    {
        Task PublishEvent(EngineNotificationRequest @event, Priority priority = Priority.None, CancellationToken cancellationToken = default);
    }
}
```

### 2.2. `IEngineConfigService.cs` & `IEngineRecoveryKeyService.cs` (Decouples `EngineController`)
* **Location**: `AIEngineConnectivity\Services\IEngineConfigService.cs` & `IEngineRecoveryKeyService.cs`
* **Implemented By**: `AIGatewayCore.EngineInfrastructure.EngineConfig` & `EngineRecoveryKeyService`
* **Consumed By**: `AIEngineGateway.Controllers.EngineController` & `AIEngineGateway.Program`
```csharp
using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.Services
{
    public interface IEngineConfigService
    {
        bool IsEngineConfig();
        string ConnectionString();
        EngineConstants.DataBaseProvider GetDatabaseType();
        Task<bool> IsDataBaseExist(EngineConstants.DataBaseProvider dataBaseProvider, string connectionString);
    }

    public interface IEngineRecoveryKeyService
    {
        string GenerateDisplayKey();
    }
}
```

### 2.3. `IServerRateLimiter.cs` & `IUserRateLimiter.cs` (Decouples Middlewares in Gateway)
* **Location**: `AIEngineConnectivity\Services\IRateLimiterServices.cs`
* **Implemented By**: `AIGatewayCore.EngineInfrastructure.RateLimiter.ServerRateLimiter` & `UserRateLimiter`
* **Consumed By**: `AIEngineGateway.Middlewares.ServerRateLimitingMiddleware` & `SpecificUserRateLimitingMiddleware`
```csharp
namespace AIEngineConnectivity.Services
{
    public interface IServerRateLimiter
    {
        bool AllowRequest();
    }

    public interface IUserRateLimiter
    {
        bool TryAddRequest(string userId);
    }
}
```

---

## 3. Rate Limiter Architecture Breakdown

### The Problem With Previous `UserRateLimiter`
1. `SpecificUserRateLimitingMiddleware` in `AIEngineGateway` was calling:
   ```csharp
   if (userId is null || !_userRateLimiter.TryAddRequest(userId))
   ```
2. But `IUserRateLimiter` originally had `bool AllowRequest(string userId);`.
3. Furthermore, `UserRateLimiter` in `AIGatewayCore` had its constructor expecting `IUserRateLimiter` instead of its backing store `UserBucketStore`, which broke the object graph.

### The Corrected Solution
1. **Contract**: `IUserRateLimiter` in `AIEngineConnectivity.Services` defines `bool TryAddRequest(string userId);`.
2. **Implementation**: `UserRateLimiter` implements `IUserRateLimiter`, and receives `UserBucketStore` in its constructor:
   ```csharp
   namespace AIEngineGateway.EngineInfrastructure.UserRateLimiter
   {
       public sealed class UserRateLimiter : IUserRateLimiter
       {
           private readonly UserBucketStore _userBucketStore;

           public UserRateLimiter(UserBucketStore userBucketStore)
           {
               _userBucketStore = userBucketStore;
           }

           public bool TryAddRequest(string userId)
           {
               return _userBucketStore.TryAddRequest(userId);
           }
       }
   }
   ```
3. **DI Registration**: In `AIGatewayCore\Extensions\ServiceExtentions.cs`:
   ```csharp
   services.AddSingleton<UserBucketStore>();
   services.AddSingleton<UserRateLimiter>();
   services.AddSingleton<IUserRateLimiter>(sp => sp.GetRequiredService<UserRateLimiter>());
   services.AddSingleton<ServerRateLimiter>();
   services.AddSingleton<IServerRateLimiter>(sp => sp.GetRequiredService<ServerRateLimiter>());
   ```
4. **Middleware**: In `AIEngineGateway\Middlewares\SpecificUserRateLimitingMiddleware.cs`:
   ```csharp
   public SpecificUserRateLimitingMiddleware(RequestDelegate next, ILogger<SpecificUserRateLimitingMiddleware> logger,
       IUserRateLimiter userRateLimiter)
   {
       _logger = logger;
       _next = next;
       _userRateLimiter = userRateLimiter;
   }
   ```

---

## 4. Build Status Across Projects

| Project | Status | Dependency Direction |
| :--- | :--- | :--- |
| **`AIEngineConnectivity`** | **BUILD SUCCEEDED** (0 errors) | Core Contracts Hub (0 dependencies) |
| **`AIEngineCore`** | **BUILD SUCCEEDED** (0 errors) | Depends **only** on `AIEngineConnectivity` |
| **`AIEngineSpeechRecognition`**| **BUILD SUCCEEDED** (0 errors) | Depends **only** on `AIEngineConnectivity` |
| **`AIGatewayCore`** | **BUILD SUCCEEDED** (0 errors) | Depends **only** on `AIEngineConnectivity` |
| **`AIEngineGateway`** | **BUILD SUCCEEDED** (0 errors) | Web Host / Composition Root (all communication through interfaces) |
| **`AIEngineUnitTest`** | **BUILD SUCCEEDED** (0 errors) | Verifies controllers and services |
