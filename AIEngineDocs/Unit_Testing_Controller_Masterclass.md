# ASP.NET Core Unit Testing Masterclass: Step-by-Step Tutorial for `ConnectionController`

Welcome to your complete, hands-on step-by-step tutorial on **Unit Testing in ASP.NET Core Web APIs**.

This guide is designed for you to follow along and write unit tests step-by-step in your project file: [ConnectionControllerTests.cs](file:///w:/Ai-workspace/ai-engine/AIEngineUnitTest/Controller/ConnectionControllerTests.cs).

---

## 📖 Table of Contents
1. [Core Unit Testing Principles](#1-core-unit-testing-principles)
2. [Deep Code Analysis of `ConnectionController`](#2-deep-code-analysis-of-connectioncontroller)
3. [Key Concepts & Framework Tools](#3-key-concepts--framework-tools)
4. [Fixing Project References & Type Resolution](#4-fixing-project-references--type-resolution)
5. [Mastering ASP.NET Core Specifics (HttpContext, ActionResults & Async)](#5-mastering-aspnet-core-specifics)
6. [Step-by-Step Tutorial: Building `ConnectionControllerTests`](#6-step-by-step-tutorial-building-connectioncontrollertests)
   - [Step 1: Declare Dependencies & System Under Test (SUT)](#step-1-declare-dependencies--system-under-test-sut)
   - [Step 2: Constructor Setup & Mock Initialization](#step-2-constructor-setup--mock-initialization)
   - [Step 3: HttpContext Mock Helper Setup](#step-3-httpcontext-mock-helper-setup)
   - [Step 4: Testing Validation & Happy Path in `GoogleCallback`](#step-4-testing-validation--happy-path-in-googlecallback)
   - [Step 5: Testing Exception Pathways & Logging](#step-5-testing-exception-pathways--logging)
   - [Step 6: Testing `SaveGoogleConnection` with HttpContext](#step-6-testing-savegoogleconnection-with-httpcontext)
   - [Step 7: Testing `TestMail` & `SaveSmtpConfiguration`](#step-7-testing-testmail--savesmtpconfiguration)
7. [Blueprint for Testing Services, Repositories & EF Core](#7-blueprint-for-testing-services-repositories--ef-core)

---

## 1. Core Unit Testing Principles

### What is a Unit Test?
A **Unit Test** verifies a single unit of code (an HTTP Action Method inside a Controller) in **complete isolation** from external systems like databases, network calls, file systems, or actual HTTP request pipelines.

```
       +-------------------------------------------------------+
       |                 ConnectionController                  |
       |             (System Under Test - SUT)                 |
       +---------------------------+---------------------------+
                                   |
                   +---------------+---------------+
                   |                               |
                   v                               v
        +---------------------+         +---------------------+
        | Fake/Mock Service   |         | Fake/Mock Logger    |
        | (In-Memory Fake)    |         | (In-Memory Fake)    |
        +---------------------+         +---------------------+
```

### The Three Golden Rules of Unit Testing:
1. **Isolated**: External dependencies (like `IEngineConnectionService` or `IEmailService`) must be replaced with **Mocks/Fakes**.
2. **Fast & Deterministic**: Tests should run in milliseconds and produce identical results on every execution.
3. **AAA Pattern**: Every test method is structured into 3 distinct phases:
   - **Arrange**: Set up test inputs, mock behaviors, and instantiate the target class (SUT).
   - **Act**: Invoke the method under test.
   - **Assert**: Verify the return value and check that expected calls were made on mocks.

---

## 2. Deep Code Analysis of `ConnectionController`

Let's inspect your target controller: [ConnectionController.cs](file:///w:/Ai-workspace/ai-engine/AIEngineGateway/Controllers/ConnectionController.cs).

```csharp
namespace AIEngineGateway.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ConnectionController : ControllerBase
    {
        private readonly IEngineConnectionService _EngineConnectionService;
        private readonly ILogger<ConnectionController> _logger;
        private readonly IEmailService _EmailService;

        public ConnectionController(
            IEngineConnectionService EngineConnectionService, 
            ILogger<ConnectionController> logger, 
            IEmailService emailService)
        {
            _EngineConnectionService = EngineConnectionService;
            _logger = logger;
            _EmailService = emailService;
        }
```

### Endpoints & Scenarios to Test:

| Endpoint | Method Signature | Scenario to Test | Expected Result |
| :--- | :--- | :--- | :--- |
| `GET api/connection/oauth/google/callback` | `GoogleCallback(code, state, cancellationToken)` | `code` is null or empty | Returns `400 BadRequest` ("Authorization code is missing.") |
| | | `state` is null or empty | Returns `400 BadRequest` ("State (UserId) is missing.") |
| | | Valid `code` & `state` | Invokes `GoogleConnectionAuthorizationCode(...)`, returns `ContentResult` (HTML) |
| | | Service throws Exception | Catches exception, logs error via `_logger`, re-throws Exception |
| `POST api/connection/saveGoogleConnection` | `SaveGoogleConnection(clientId, clientSecret, cancellationToken)` | Reads `Request.Scheme` & `Request.Host` | Passes HTTP scheme/host to service, returns `200 OK` |
| `POST api/connection/testMail` | `TestMail(smtpConfiguration, cancellationToken)` | Valid SMTP DTO | Invokes `_EmailService.SendTestMail(...)`, returns `200 OK` |
| `POST api/connection/savesmtpConfiguration` | `SaveSmtpConfiguration(smtpConfiguration, cancellationToken)` | Valid SMTP DTO | Invokes `_EngineConnectionService.SaveSmtpConfiguration(...)`, returns `200 OK` |

---

## 3. Key Concepts & Framework Tools

In your test project (`AIEngineUnitTest.csproj`), you have the following essential testing libraries:

1. **xUnit** (`Xunit`): The test runner framework.
   - `[Fact]`: Defines a unit test method with fixed inputs.
   - `[Theory]`: Defines a parameterized test method that executes multiple times (`[InlineData(...)]`).
2. **FakeItEasy** / **Moq**: Mocking libraries to create fake instances of interfaces (`IEngineConnectionService`, `IEmailService`, `ILogger`).
   - FakeItEasy syntax: `A.Fake<IEmailService>()`
   - Setting fake behavior: `A.CallTo(() => service.Method()).Returns(...)`
   - Verifying execution: `A.CallTo(() => service.Method()).MustHaveHappenedOnceExactly()`
3. **FluentAssertions**: Provides clear, readable assertion syntax.
   - `result.Should().BeOfType<OkResult>()`
   - `badRequest.StatusCode.Should().Be(400)`

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
ASP.NET Core Controllers use `ILogger<ConnectionController>` from `Microsoft.Extensions.Logging` (not Castle.Core or generic `ILogger`):
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

Now open your file [ConnectionControllerTests.cs](file:///w:/Ai-workspace/ai-engine/AIEngineUnitTest/Controller/ConnectionControllerTests.cs) and follow these steps to construct your test suite!

---

### Step 1: Declare Dependencies & System Under Test (SUT)

First, add necessary namespaces and declare private fields for all 3 dependencies of `ConnectionController`, plus a field for the controller itself (`_sut`).

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
```

---

### Step 2: Constructor Setup & Mock Initialization

In xUnit, the class **constructor** runs before **every single test method**. This ensures each test starts with a fresh, clean set of mocks.

```csharp
        public ConnectionControllerTests()
        {
            // Create Fake instances for all dependencies using FakeItEasy
            _engineConnectionService = A.Fake<IEngineConnectionService>();
            _logger = A.Fake<ILogger<ConnectionController>>();
            _emailService = A.Fake<IEmailService>();

            // Instantiate the System Under Test (SUT)
            _sut = new ConnectionController(_engineConnectionService, _logger, _emailService);
        }
```

---

### Step 3: HttpContext Mock Helper Setup

Add a helper method inside `ConnectionControllerTests` to configure `ControllerContext` when a test method needs to exercise endpoints that access `Request.Scheme` or `Request.Host`.

```csharp
        #region Helper Methods

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

        #endregion
```

---

### Step 4: Testing Validation & Happy Path in `GoogleCallback`

Now write the tests for `GoogleCallback`:
1. When `code` is null/empty -> returns `BadRequestObjectResult`.
2. When `state` is null/empty -> returns `BadRequestObjectResult`.
3. When both are valid -> calls service and returns `ContentResult` (HTML).

```csharp
        #region GoogleCallback Tests

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

            // Verify dependency was NOT invoked
            A.CallTo(() => _engineConnectionService.GoogleConnectionAuthorizationCode(
                A<string>._, A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task GoogleCallback_WhenStateIsMissing_ReturnsBadRequest()
        {
            // ARRANGE
            string code = "valid-auth-code";
            string state = null;

            // ACT
            var result = await _sut.GoogleCallback(code, state, CancellationToken.None);

            // ASSERT
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("State (UserId) is missing.");

            A.CallTo(() => _engineConnectionService.GoogleConnectionAuthorizationCode(
                A<string>._, A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task GoogleCallback_WhenValidParametersProvided_CallsServiceAndReturnsHtmlContent()
        {
            // ARRANGE
            string code = "google-auth-code-xyz";
            string state = "user-456";
            var cancellationToken = CancellationToken.None;

            // ACT
            var result = await _sut.GoogleCallback(code, state, cancellationToken);

            // ASSERT
            var contentResult = result.Should().BeOfType<ContentResult>().Subject;
            contentResult.ContentType.Should().Be("text/html");
            contentResult.Content.Should().Contain("google-drive-connected");

            // VERIFY interaction with service
            A.CallTo(() => _engineConnectionService.GoogleConnectionAuthorizationCode(code, state, cancellationToken))
                .MustHaveHappenedOnceExactly();
        }
```

---

### Step 5: Testing Exception Pathways & Logging

Test what happens when `_engineConnectionService` throws an Exception inside `GoogleCallback`.

```csharp
        [Fact]
        public async Task GoogleCallback_WhenServiceThrowsException_LogsErrorAndRethrows()
        {
            // ARRANGE
            string code = "valid-code";
            string state = "valid-state";
            var cancellationToken = CancellationToken.None;
            var expectedException = new Exception("Google API OAuth failed");

            // Configure fake service to throw an exception
            A.CallTo(() => _engineConnectionService.GoogleConnectionAuthorizationCode(code, state, cancellationToken))
                .Throws(expectedException);

            // ACT & ASSERT
            Func<Task> action = async () => await _sut.GoogleCallback(code, state, cancellationToken);

            await action.Should().ThrowAsync<Exception>()
                .WithMessage("Google API OAuth failed");
        }

        #endregion
```

---

### Step 6: Testing `SaveGoogleConnection` with HttpContext

Write a test for `SaveGoogleConnection`. Use `SetupMockHttpContext` to verify that `Request.Scheme` and `Request.Host` are passed to `SaveAndConnectGoogleConnection`.

```csharp
        #region SaveGoogleConnection Tests

        [Fact]
        public async Task SaveGoogleConnection_WhenCalled_PassesHttpContextDetailsToServiceAndReturnsOk()
        {
            // ARRANGE
            SetupMockHttpContext(scheme: "https", host: "api.aiengine.com");
            string clientId = "client-id-123";
            string clientSecret = "client-secret-xyz";
            var cancellationToken = CancellationToken.None;

            // ACT
            var result = await _sut.SaveGoogleConnection(clientId, clientSecret, cancellationToken);

            // ASSERT
            result.Should().BeOfType<OkResult>();

            // VERIFY that Request.Scheme ("https") and Request.Host ("api.aiengine.com") were correctly passed
            A.CallTo(() => _engineConnectionService.SaveAndConnectGoogleConnection(
                clientId,
                clientSecret,
                "https",
                "api.aiengine.com",
                cancellationToken
            )).MustHaveHappenedOnceExactly();
        }

        #endregion
```

---

### Step 7: Testing `TestMail` & `SaveSmtpConfiguration`

Finally, add test methods for `TestMail` and `SaveSmtpConfiguration`.

```csharp
        #region TestMail Tests

        [Fact]
        public async Task TestMail_WhenCalledWithValidConfig_CallsEmailServiceAndReturnsOk()
        {
            // ARRANGE
            var smtpConfig = new SmtpConfiguration
            {
                Host = "smtp.gmail.com",
                Port = 587,
                Username = "test@example.com",
                Password = "secret-password",
                EnableSsl = true
            };
            var cancellationToken = CancellationToken.None;

            // ACT
            var result = await _sut.TestMail(smtpConfig, cancellationToken);

            // ASSERT
            result.Should().BeOfType<OkResult>();

            // VERIFY execution on email service fake
            A.CallTo(() => _emailService.SendTestMail(smtpConfig))
                .MustHaveHappenedOnceExactly();
        }

        #endregion

        #region SaveSmtpConfiguration Tests

        [Fact]
        public async Task SaveSmtpConfiguration_WhenCalledWithValidConfig_CallsEngineConnectionServiceAndReturnsOk()
        {
            // ARRANGE
            var smtpConfig = new SmtpConfiguration
            {
                Host = "smtp.office365.com",
                Port = 587,
                Username = "admin@aiengine.com",
                Password = "admin-password",
                EnableSsl = true
            };
            var cancellationToken = CancellationToken.None;

            // ACT
            var result = await _sut.SaveSmtpConfiguration(smtpConfig, cancellationToken);

            // ASSERT
            result.Should().BeOfType<OkResult>();

            // VERIFY execution on engine connection service fake
            A.CallTo(() => _engineConnectionService.SaveSmtpConfiguration(smtpConfig, cancellationToken))
                .MustHaveHappenedOnceExactly();
        }

        #endregion
    }
}
```

---

## 7. Blueprint for Testing Services, Repositories & EF Core

Once you finish implementing unit tests for all controllers, here is how you test **Services** and **Entity Framework Core (EF Core)**:

### 1. Unit Testing Services
- In service tests, your **System Under Test (SUT)** is the Service class itself (e.g., `EngineConnectionService`).
- Mock any underlying repositories, database contexts, or external HTTP clients.
- Assert business calculations, data mapping, and validation rules.

### 2. Unit Testing EF Core Repositories
- For Entity Framework Core, do **NOT** mock `DbContext` with FakeItEasy or Moq.
- Instead, use EF Core's **In-Memory Database**:

```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;

using (var context = new ApplicationDbContext(options))
{
    // Seed test data into In-Memory database
    context.Users.Add(new User { Id = "1", Email = "test@example.com" });
    context.SaveChanges();
}

using (var context = new ApplicationDbContext(options))
{
    // Act & Assert against in-memory repository
    var repo = new UserRepository(context);
    var user = await repo.GetByIdAsync("1");
    user.Should().NotBeNull();
}
```

---

## 🎓 Summary Checklist for Writing Your Tests

- [x] Project reference added to `AIEngineUnitTest.csproj`.
- [x] Correct `using AIEngineGateway.Controllers;` and `Microsoft.Extensions.Logging` added.
- [ ] Add constructor and initialize `A.Fake<T>()` for dependencies.
- [ ] Add `SetupMockHttpContext(...)` helper.
- [ ] Implement Arrange-Act-Assert for each action method.
- [ ] Run `dotnet test` or use Visual Studio Test Explorer to execute tests.
