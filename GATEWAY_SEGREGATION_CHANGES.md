# AIEngineGateway & AIGatewayCore Architectural Segregation Guide

## Executive Summary
This document outlines the architecture segregation between **`AIEngineGateway`** (Web API Host / Presentation Layer) and **`AIGatewayCore`** (Domain Business Logic, Repositories, EF Core Contexts, Background Services, SignalR Hubs). 

Following the migration of files out of `AIEngineGateway` into `AIGatewayCore`, this guide details all structural problems, broken links, circular references, runtime hazards, and exact code changes required to restore a clean, buildable, and maintainable solution.

---

## 1. System Architecture & Dependency Graph

### Dependency Flow (Strictly Unidirectional)
```
                  ┌──────────────────────┐
                  │ AIEngineConnectivity │
                  └──────────▲───────────┘
                             │
            ┌────────────────┴────────────────┐
            │                                 │
  ┌──────────────────┐             ┌─────────────────────┐
  │   AIEngineCore   │             │AIEngineSpeechRecogn.│
  └─────────▲────────┘             └──────────▲──────────┘
            │                                 │
            └────────────────┬────────────────┘
                             │
                  ┌──────────────────────┐
                  │    AIGatewayCore     │
                  └──────────▲───────────┘
                             │
                  ┌──────────────────────┐
                  │   AIEngineGateway    │ (Web API Host)
                  └──────────▲───────────┘
                             │
                  ┌──────────────────────┐
                  │   AIEngineInstaller  │
                  └──────────────────────┘
```

### Responsibility Matrix
| Project | Responsibilities | Target Framework / Sdk |
| :--- | :--- | :--- |
| **`AIEngineConnectivity`** | Shared Contracts, Interfaces, DTOs, Database Models/Entities, Constants. Has zero project dependencies. | `net10.0` (Microsoft.NET.Sdk) |
| **`AIEngineCore`** | AI Provider Routers (Gemini, Groq, Ollama, Cohere, etc.), Priority Pub/Sub Notification Bus, Email Workers, Template Providers. | `net10.0` (Microsoft.NET.Sdk) |
| **`AIEngineSpeechRecognition`** | Audio processing, Whisper.net STT integration, FFmpeg audio transcoding. | `net10.0` (Microsoft.NET.Sdk) |
| **`AIGatewayCore`** | EF Core DbContexts (`SqlServerEngineContext`, `PostgreSqlEngineContext`), Repositories, Quartz Background Jobs & Schedulers, Rate Limiters, SignalR Hubs (`NotificationHub`, `EngineStatusHub`), Core Domain Services (`IdentityService`, `ConversationService`, `EngineDriveService`, etc.). | `net10.0` (Microsoft.NET.Sdk with `Microsoft.AspNetCore.App` framework ref) |
| **`AIEngineGateway`** | ASP.NET Core Web API Host (`Program.cs`), API Controllers (`Controllers/`), Static File Hosting for Angular SPA (`wwwroot/`), Notification Templates (`Templates/`), Configuration (`appsettings.json`). | `net10.0` (Microsoft.NET.Sdk.Web) |

---

## 2. Critical Blockers & Hazards Identified

### Blocker 1: Circular Dependency Hazard (MSBuild `MSB4006`)
* **Current State**: `AIGatewayCore.csproj` has a reference to `..\..\AIEngineGateway\AIEngineGateway.csproj`, while `AIEngineGateway` requires `AIGatewayCore`.
* **Root Cause**: `MiddlewareExtensions.cs` was moved to `AIGatewayCore`, but the concrete middleware classes (`GlobalExceptionMiddleware`, `ServerRateLimitingMiddleware`, etc.) were left in `AIEngineGateway\Middlewares\`.
* **Fix**: Remove `AIEngineGateway.csproj` reference from `AIGatewayCore.csproj`. Resolve the middleware placement (see Step 2).

### Blocker 2: Missing Speech Recognition Dependency in `AIGatewayCore`
* **Current State**: `ServiceExtentions.cs` in `AIGatewayCore` calls `services.AddSingleton<IWhisperService, WhisperService>()`, but `AIGatewayCore.csproj` does NOT reference `AIEngineSpeechRecognition.csproj`.
* **Fix**: Add `AIEngineSpeechRecognition` project reference to `AIGatewayCore.csproj`.

### Blocker 3: Quartz Database Scripts Not Embedded (`FileNotFoundException`)
* **Current State**: In `AIGatewayCore\EngineInfrastructure\DatabaseScripts\ScriptLoader.cs`:
  ```csharp
  var resourceName = $"AIEngineGateway.EngineInfrastructure.DatabaseScripts.{scriptName}";
  ```
  The SQL files were moved to `AIGatewayCore`, but `AIGatewayCore.csproj` does **not** have `<EmbeddedResource Include="EngineInfrastructure\DatabaseScripts\*.sql" />`.
* **Consequence**: When Quartz initializes, `assembly.GetManifestResourceStream()` will return `null` and throw a runtime `FileNotFoundException`.
* **Fix**: Add `<EmbeddedResource>` to `AIGatewayCore.csproj` and make `ScriptLoader.cs` use dynamic assembly naming.

---

## 3. Step-by-Step File Changes

### Step 1: Fix `AIGatewayCore\AIGatewayCore\AIGatewayCore.csproj`

**File Path**: `AIGatewayCore\AIGatewayCore\AIGatewayCore.csproj`

1. Remove the circular project reference to `AIEngineGateway.csproj`.
2. Add reference to `AIEngineSpeechRecognition.csproj`.
3. Add `<EmbeddedResource>` for the Quartz SQL scripts.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.5" />
    <PackageReference Include="Microsoft.AspNetCore.DataProtection" Version="10.0.13" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.5">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.Identity.Core" Version="10.0.5" />
    <PackageReference Include="Microsoft.Extensions.Identity.Stores" Version="10.0.5" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.2" />
    <PackageReference Include="Quartz" Version="3.19.1" />
    <PackageReference Include="Quartz.Extensions.DependencyInjection" Version="3.19.1" />
    <PackageReference Include="Quartz.Extensions.Hosting" Version="3.19.1" />
    <PackageReference Include="Quartz.Serialization.SystemTextJson" Version="3.18.2" />
    <PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
  </ItemGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\AIEngineConnectivity\AIEngineConnectivity.csproj" />
    <ProjectReference Include="..\..\AIEngineCore\AIEngineCore.csproj" />
    <ProjectReference Include="..\..\AIEngineSpeechRecognition\AIEngineSpeechRecognition.csproj" />
  </ItemGroup>

  <ItemGroup>
    <EmbeddedResource Include="EngineInfrastructure\DatabaseScripts\*.sql" />
  </ItemGroup>

</Project>
```

---

### Step 2: Resolve Middlewares Placement (Eliminating Inversion)

There are two valid architectural approaches. **Option A is strongly recommended**.

#### Option A (Recommended): Move `Middlewares/` to `AIGatewayCore`
Move the folder `AIEngineGateway\Middlewares\` to `AIGatewayCore\AIGatewayCore\Middlewares\`.
* **Reasoning**:
  - `ServerRateLimitingMiddleware` and `SpecificUserRateLimitingMiddleware` depend directly on `ServerRateLimiter` and `UserRateLimiter` (which live in `AIGatewayCore`).
  - `EncryptionMiddleware` depends on `EncryptionService` (which lives in `AIGatewayCore`).
  - `CurrentUserContextMiddleWare` depends on `IUserSessionManager` (which lives in `AIGatewayCore`).
  - `MiddlewareExtensions.cs` already lives in `AIGatewayCore\Extensions\MiddlewareExtensions.cs`.
  - `AIGatewayCore` already includes `<FrameworkReference Include="Microsoft.AspNetCore.App" />`, so ASP.NET Core middleware types are fully supported.
  - This leaves `AIEngineGateway` strictly as an API presentation host.

#### Option B (Alternative): Keep Middlewares in `AIEngineGateway`
If you choose to keep middlewares in `AIEngineGateway`:
1. Move `AIGatewayCore\AIGatewayCore\Extensions\MiddlewareExtensions.cs` back to `AIEngineGateway\Extensions\MiddlewareExtensions.cs`.
2. In `ServerRateLimitingMiddleware.cs` & `SpecificUserRateLimitingMiddleware.cs`, add:
   ```csharp
   using AIEngineGateway.EngineInfrastructure.RateLimiter;
   ```
3. In `EncryptionMiddleware.cs`, add:
   ```csharp
   using AIEngineGateway.Services;
   ```

---

### Step 3: Fix Quartz Embedded Script Loader

**File Path**: `AIGatewayCore\AIGatewayCore\EngineInfrastructure\DatabaseScripts\ScriptLoader.cs`

Replace hardcoded `"AIEngineGateway..."` with the dynamic assembly name:

```csharp
namespace AIEngineGateway.EngineInfrastructure.DatabaseScripts
{
    public static class ScriptLoader
    {
        public static async Task<string> LoadScriptsAsync(string scriptName)
        {
            var assembly = typeof(ScriptLoader).Assembly;
            var assemblyName = assembly.GetName().Name;
            var resourceName = $"{assemblyName}.EngineInfrastructure.DatabaseScripts.{scriptName}";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
                throw new FileNotFoundException($"Embedded SQL resource '{resourceName}' was not found in assembly '{assembly.FullName}'.");
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
```

---

### Step 4: Update `AIEngineGateway\AIEngineGateway.csproj`

**File Path**: `AIEngineGateway\AIEngineGateway.csproj`

1. Add `<ProjectReference Include="..\AIGatewayCore\AIGatewayCore\AIGatewayCore.csproj" />`.
2. Convert file-based DLL references (`..\AIEngineConnectivity\...` and `libs\AIEngineCore.dll`) to project references (or remove them, since `AIGatewayCore` transitively brings them).
3. Remove stale `<EmbeddedResource Include="EngineInfrastructure\DatabaseScripts\*.sql" />`.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <UserSecretsId>4fb817f8-c2df-4502-bd97-007e325f0216</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.5" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version="10.0.7" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.5" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.2.9" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="10.0.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
    <PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.5" />
    <PackageReference Include="Google.GenAI" Version="1.0.0-beta.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\AIGatewayCore\AIGatewayCore\AIGatewayCore.csproj" />
    <ProjectReference Include="..\AIEngineSpeechRecognition\AIEngineSpeechRecognition.csproj" />
  </ItemGroup>

  <ItemGroup>
    <EmbeddedResource Include="Templates\**\*.*" />
  </ItemGroup>

</Project>
```

---

### Step 5: Update `AIEngineGateway\Controllers\EngineController.cs`

**File Path**: `AIEngineGateway\Controllers\EngineController.cs`

Add the missing `using AIEngineGateway.EngineInfrastructure;` so that `EngineConfig` and `EngineRecoveryKeyService` resolve:

```csharp
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Services;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.AspNetCore.Mvc;

namespace AIEngineGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EngineController : ControllerBase
    {
        private readonly EngineConfig _engineConfig;
        private readonly ILogger<EngineController> _logger;
        private readonly IEngineDataBaseService _engineDataBaseService;
        private readonly EngineRecoveryKeyService _engineRecoveryKeyService;

        public EngineController(EngineConfig engineConfig, IEngineDataBaseService engineDataBaseService,
            ILogger<EngineController> logger, EngineRecoveryKeyService engineRecoveryKeyService)
        {
            _engineDataBaseService = engineDataBaseService;
            _engineConfig = engineConfig;
            _logger = logger;
            _engineRecoveryKeyService = engineRecoveryKeyService;
        }

        [HttpGet("engine-status")]
        public async Task<ActionResult<SystemStatusResponse>> GetStatusOfEngine()
        {
            bool isEngineConfig = _engineConfig.IsEngineConfig();
            bool isDataBaseExists = false;
            if (isEngineConfig)
            {
                var connectionString = _engineConfig.ConnectionString();
                isDataBaseExists = await _engineConfig.IsDataBaseExist(_engineConfig.GetDatabaseType(), connectionString);
            }
            return Ok(new SystemStatusResponse
            {
                IsDataBaseConfigure = isEngineConfig && isDataBaseExists
            });
        }

        [HttpPost("configure-database")]
        public async Task<ActionResult> ConfigureDataBase(DataBaseConfiguration dataBaseConfiguration)
        {
            try
            {
                await _engineDataBaseService.ConfigureDataBase(dataBaseConfiguration);
                return Ok(new { Message = "Database Configuration Successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while configuring database");
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
```

---

### Step 6: Verify `AIEngineGateway\Program.cs`

**File Path**: `AIEngineGateway\Program.cs`

With `AIGatewayCore` referenced, `Program.cs` works cleanly with no code changes needed:

```csharp
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
```

---

### Step 7: Update `build.cmd` Build Pipeline

**File Path**: `build.cmd`

The build order must compile `gateway_core` **before** `gateway`. Also ensure `AIGatewayCore.dll` is replicated to `libs\` folders if any projects rely on file-based references.

Update `build_all` and `:build_gateway_core`:

```cmd
:build_all
call :build_connectivity
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_core
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_speech
if !errorlevel! neq 0 exit /b !errorlevel!
if exist "%GATEWAY_CORE_SLN%" (
    call :build_gateway_core
    if !errorlevel! neq 0 exit /b !errorlevel!
)
call :build_client
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_gateway
if !errorlevel! neq 0 exit /b !errorlevel!
call :build_installer
if !errorlevel! neq 0 exit /b !errorlevel!
echo Build completed successfully for target: all
goto end
```

And update the `:build_gateway_core` routine:

```cmd
:build_gateway_core
echo.
echo Building AIGatewayCore (%CONFIG%)...
dotnet build "%GATEWAY_CORE_SLN%" -c "%CONFIG%"
if !errorlevel! neq 0 exit /b !errorlevel!
echo Updating AIGatewayCore references in dependent solutions...
if not exist "AIEngineGateway\libs" mkdir "AIEngineGateway\libs"
if not exist "AIEngineInstaller\libs" mkdir "AIEngineInstaller\libs"
copy /Y "AIGatewayCore\AIGatewayCore\bin\%CONFIG%\net10.0\AIGatewayCore.*" "AIEngineGateway\libs\"
copy /Y "AIGatewayCore\AIGatewayCore\bin\%CONFIG%\net10.0\AIGatewayCore.*" "AIEngineInstaller\libs\"
goto :eof
```

---

### Step 8: Update Solution Registration (`AIEngineGateway.slnx`)

**File Path**: `AIEngineGateway\AIEngineGateway.slnx`

Add `AIGatewayCore` so that Visual Studio shows both projects under the Gateway solution:

```xml
<Solution>
  <Project Path="../AIEngineUnitTest/AIEngineUnitTest.csproj" Id="55ad767b-b8df-4fc5-a841-5b43658b30c0" />
  <Project Path="../AIGatewayCore/AIGatewayCore/AIGatewayCore.csproj" />
  <Project Path="AIEngineGateway.csproj" />
</Solution>
```

---

## 4. Verification Checklist

- [ ] `AIGatewayCore.csproj` no longer has `<ProjectReference>` pointing to `AIEngineGateway.csproj`.
- [ ] `AIGatewayCore.csproj` has `<ProjectReference>` pointing to `AIEngineSpeechRecognition.csproj`.
- [ ] `AIGatewayCore.csproj` has `<EmbeddedResource Include="EngineInfrastructure\DatabaseScripts\*.sql" />`.
- [ ] `ScriptLoader.cs` dynamically uses `assembly.GetName().Name` for resource resolution.
- [ ] Middlewares are moved to `AIGatewayCore\AIGatewayCore\Middlewares\` (or `MiddlewareExtensions.cs` returned to `AIEngineGateway`).
- [ ] `AIEngineGateway.csproj` references `AIGatewayCore.csproj`.
- [ ] `EngineController.cs` has `using AIEngineGateway.EngineInfrastructure;`.
- [ ] `build.cmd` compiles in order: `connectivity` -> `core` -> `speech` -> `gateway_core` -> `client` -> `gateway` -> `installer`.
- [ ] Run `dotnet build AIGatewayCore\AIGatewayCore\AIGatewayCore.csproj` to confirm 0 errors.
- [ ] Run `dotnet build AIEngineGateway\AIEngineGateway.csproj` to confirm 0 errors.

