# ASP.NET Core Unit Testing Masterclass: Step-by-Step Tutorial & Controller Deep Review

Welcome to your complete, hands-on masterclass on **Unit Testing in ASP.NET Core Web APIs**.

This guide provides a deep-dive review of **all Controllers** in your solution, explains the **Solution Unit Test Structure (`.slnx`)**, and teaches you how to run and write unit tests step-by-step in your project test file: [ConnectionControllerTests.cs](file:///w:/Ai-workspace/ai-engine/AIEngineUnitTest/Controller/ConnectionControllerTests.cs).

---

## 📖 Table of Contents
1. [Solution Architecture & Test Project Mapping (`.slnx`)](#1-solution-architecture--test-project-mapping-slnx)
2. [Deep Review Map of Gateway Controllers](#2-deep-review-map-of-gateway-controllers)
3. [Core Unit Testing Principles](#3-core-unit-testing-principles)
4. [Fixing Project References & Type Resolution](#4-fixing-project-references--type-resolution)
5. [Mastering ASP.NET Core Specifics (HttpContext, ActionResults & Async)](#5-mastering-aspnet-core-specifics)
6. [Step-by-Step Tutorial: Building `ConnectionControllerTests`](#6-step-by-step-tutorial-building-connectioncontrollertests)
7. [How to Run All Tests in the Solution](#7-how-to-run-all-tests-in-the-solution)
8. [Blueprint for Testing Services, Repositories & EF Core](#8-blueprint-for-testing-services-repositories--ef-core)

---

## 1. Solution Architecture & Test Project Mapping (`.slnx`)

Your project solution uses Visual Studio's modern `.slnx` solution file: [AIEngineGateway.slnx](file:///w:/Ai-workspace/ai-engine/AIEngineGateway/AIEngineGateway.slnx).

```
ai-engine/
├── AIEngineGateway/ (Main Web API Project - net10.0)
│   ├── Controllers/
│   │   ├── ConnectionController.cs
│   │   ├── DashboardController.cs
│   │   ├── EncryptionController.cs
│   │   ├── EngineController.cs
│   │   ├── EngineStateController.cs
│   │   ├── ConversationController.cs
│   │   └── IdentityController.cs
│   └── AIEngineGateway.slnx <--- Solution linking both projects
│
└── AIEngineUnitTest/ (Unit Test Suite Project - net10.0)
    ├── Controller/
    │   ├── ConnectionControllerTests.cs <--- Target Test File
    │   ├── EngineControllerTests.cs     <--- Recommended Next Test
    │   ├── EncryptionControllerTests.cs <--- Recommended Next Test
    │   └── ...
    └── AIEngineUnitTest.csproj <--- ProjectReference to AIEngineGateway
```

---

## 2. Deep Review Map of Gateway Controllers

Here is an architectural review of all controllers in `AIEngineGateway` and what dependencies you must mock when testing each one:

### 1. `ConnectionController` ([ConnectionController.cs](file:///w:/Ai-workspace/ai-engine/AIEngineGateway/Controllers/ConnectionController.cs))
- **Purpose**: Handles Google OAuth callback, saving Google credentials, testing SMTP mail settings, and saving SMTP config.
- **Dependencies to Mock**:
  - `IEngineConnectionService`: For OAuth token exchange & saving configuration.
  - `IEmailService`: For sending test emails.
  - `ILogger<ConnectionController>`: For logging errors.
- **Special Requirement**: Uses `Request.Scheme` and `Request.Host` in `SaveGoogleConnection`, so tests must set `ControllerContext.HttpContext`.

### 2. `EngineController` ([EngineController.cs](file:///w:/Ai-workspace/ai-engine/AIEngineGateway/Controllers/EngineController.cs))
- **Purpose**: Manages system & database configuration status, database setup, and database connectivity test.
- **Dependencies to Mock**:
  - `EngineConfig`: System configuration helper.
  - `IEngineDataBaseService`: Database configuration & testing service.
  - `ILogger<EngineController>`: For logging warnings and errors.
- **Key Test Cases**:
  - `GetStatusOfEngine`: Tests `IsEngineConfig()` and `IsDataBaseExist()` logic returning `SystemStatusResponse`.
  - `ConfigureDataBase`: Tests `OkResult` on success or `BadRequest` on exception.
  - `TestConnectionAsync`: Tests success message vs failure message.

### 3. `EncryptionController` ([EncryptionController.cs](file:///w:/Ai-workspace/ai-engine/AIEngineGateway/Controllers/EncryptionController.cs))
- **Purpose**: Exposes API endpoint for fetching system public key for client payload encryption.
- **Dependencies to Mock**:
  - `IEncryptionService`: For generating/retrieving public key.
  - `ILogger<EncryptionController>`: Logger instance.
- **Key Test Case**:
  - `GetPublicKey`: Verifies string return wrapped in `OkObjectResult`.

### 4. `DashboardController` ([DashboardController.cs](file:///w:/Ai-workspace/ai-engine/AIEngineGateway/Controllers/DashboardController.cs))
- **Purpose**: Fetches supported AI model providers catalog (`ModelCatalog.Providers`).
- **Dependencies to Mock**:
  - `ILogger<DashboardController>`: Logger instance.
- **Key Test Case**:
  - `GetModels`: Returns list of AI providers catalog.

### 5. `EngineStateController` ([EngineStateController.cs](file:///w:/Ai-workspace/ai-engine/AIEngineGateway/Controllers/EngineStateController.cs))
- **Purpose**: Returns real-time state of the AI engine (running state, ready state, error message).
- **Dependencies to Mock**:
  - `EngineState`: State container object.
- **Key Test Case**:
  - `GetEngineState`: Verifies anonymous object containing `isEngineRunning`, `isEngineReady`, and `errorMessage`.

---

## 3. Core Unit Testing Principles

### What is a Unit Test?
A **Unit Test** verifies a single unit of code (an HTTP Action Method inside a Controller) in **complete isolation** from external systems like databases, network calls, file systems, or actual HTTP request pipelines.

### The Three Golden Rules of Unit Testing:
1. **Isolated**: External dependencies (like `IEngineConnectionService` or `IEmailService`) must be replaced with **Mocks/Fakes**.
2. **Fast & Deterministic**: Tests should run in milliseconds and produce identical results on every execution.
3. **AAA Pattern**: Every test method is structured into 3 distinct phases:
   - **Arrange**: Set up test inputs, mock behaviors, and instantiate the target class (SUT).
   - **Act**: Invoke the method under test.
   - **Assert**: Verify the return value and check that expected calls were made on mocks.

---

## 4. Fixing Project References & Type Resolution

If Visual Studio displays an error like `The type or namespace name 'ConnectionController' could not be found`, check these 3 essential configurations:

### 1. Project Reference in `.csproj`
Your test project `AIEngineUnitTest.csproj` must reference your Web API project `AIEngineGateway.csproj`:
```xml
<ItemGroup>
  <ProjectReference Include="..\AIEngineGateway\AIEngineGateway.csproj" />
</ItemGroup>
```

### 2. Namespace Imports in Test File
Make sure your test file imports the namespace where `ConnectionController` lives:
```csharp
using AIEngineGateway.Controllers;
```

### 3. Proper Logger Type Interface
ASP.NET Core Controllers use `ILogger<ConnectionController>` from `Microsoft.Extensions.Logging`:
```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<ConnectionController> _logger;
```

---

## 5. Mastering ASP.NET Core Specifics

### Crucial Lesson: Mocking `HttpContext` and `ControllerContext`

Look closely at `SaveGoogleConnection` in `ConnectionController`:
```csharp
[HttpPost("saveGoogleConnection")]
public async Task<IActionResult> SaveGoogleConnection([FromQuery] string clientId, string clientSecret, CancellationToken cancellationToken)
{
    await _EngineConnectionService.SaveAndConnectGoogleConnection(
        clientId, 
        clientSecret, 
        Request.Scheme,         // <-- Accesses ControllerBase.Request
        Request.Host.ToString(),// <-- Accesses ControllerBase.Request
        cancellationToken);
    return Ok();
}
```

#### ⚠️ The Common Pitfall:
When you instantiate `new ConnectionController(...)` directly in a unit test, ASP.NET Core's request pipeline has not executed. Therefore, `ControllerContext` and `HttpContext` are `null`. Calling `Request.Scheme` will throw a `NullReferenceException`!

#### ✅ The Solution:
In your test setup, manually attach a `DefaultHttpContext` to `ControllerContext`:
```csharp
private void SetupMockHttpContext(string scheme = "https", string host = "localhost:5001")
{
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Scheme = scheme;
    httpContext.Request.Host = new HostString(host);

    _sut.ControllerContext = new ControllerContext
    {
        HttpContext = httpContext
    };
}
```

---

## 6. Step-by-Step Tutorial: Building `ConnectionControllerTests`

Open [ConnectionControllerTests.cs](file:///w:/Ai-workspace/ai-engine/AIEngineUnitTest/Controller/ConnectionControllerTests.cs) to build your test suite step-by-step!

```csharp
namespace AIEngineUnitTest.Controller
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Services;
    using AIEngineGateway.Controllers;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class ConnectionControllerTests
    {
        // 1. Mock Dependencies
        private readonly IEngineConnectionService _engineConnectionService;
        private readonly ILogger<ConnectionController> _logger;
        private readonly IEmailService _emailService;

        // 2. System Under Test (SUT)
        private readonly ConnectionController _sut;

        public ConnectionControllerTests()
        {
            // Initialize Fakes using FakeItEasy
            _engineConnectionService = A.Fake<IEngineConnectionService>();
            _logger = A.Fake<ILogger<ConnectionController>>();
            _emailService = A.Fake<IEmailService>();

            // Instantiate SUT
            _sut = new ConnectionController(_engineConnectionService, _logger, _emailService);
        }

        private void SetupMockHttpContext(string scheme = "https", string host = "localhost:5001")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = scheme;
            httpContext.Request.Host = new HostString(host);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task GoogleCallback_WhenCodeIsMissing_ReturnsBadRequest()
        {
            // ARRANGE
            string code = "";
            string state = "user-123";

            // ACT
            var result = await _sut.GoogleCallback(code, state, CancellationToken.None);

            // ASSERT
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Authorization code is missing.");
        }

        [Fact]
        public async Task SaveGoogleConnection_WhenCalled_PassesHttpContextDetailsToServiceAndReturnsOk()
        {
            // ARRANGE
            SetupMockHttpContext(scheme: "https", host: "api.aiengine.com");
            string clientId = "client-id-123";
            string clientSecret = "client-secret-xyz";

            // ACT
            var result = await _sut.SaveGoogleConnection(clientId, clientSecret, CancellationToken.None);

            // ASSERT
            result.Should().BeOfType<OkResult>();

            A.CallTo(() => _engineConnectionService.SaveAndConnectGoogleConnection(
                clientId, clientSecret, "https", "api.aiengine.com", A<CancellationToken>._
            )).MustHaveHappenedOnceExactly();
        }
    }
}
```

---

## 7. How to Run All Tests in the Solution

You can run and discover all tests in the solution using any of the following methods:

### Option A: Using .NET CLI (Command Line / Terminal)
Navigate to `AIEngineGateway` or root and run:
```bash
# Run all tests in the test project
dotnet test AIEngineUnitTest/AIEngineUnitTest.csproj

# Or run tests using the solution file
dotnet test AIEngineGateway/AIEngineGateway.slnx
```

### Option B: Using Visual Studio Test Explorer
1. Open [AIEngineGateway.slnx](file:///w:/Ai-workspace/ai-engine/AIEngineGateway/AIEngineGateway.slnx) in Visual Studio.
2. Go to top menu: **Test -> Test Explorer** (Ctrl + E, T).
3. Click **Run All Tests** (Green Play button). All tests in `AIEngineUnitTest` will run and display green checkmarks!

### Option C: Using VS Code
1. Install `.NET Core Test Explorer` or `C# Dev Kit` extension.
2. Click the **Testing** tab (Flask icon on left bar) to discover and run tests with 1-click.

---

## 8. Blueprint for Testing Services, Repositories & EF Core

- **Services**: Mock repository interfaces and verify domain logic.
- **EF Core Repositories**: Use EF Core's `UseInMemoryDatabase`:

```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;

using var context = new ApplicationDbContext(options);
// Perform repository test against in-memory database
```
