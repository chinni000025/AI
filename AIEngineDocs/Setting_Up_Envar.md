##  Engine Secret Keys: Quick Start Guide

This guide outlines the essential steps to initialize and configure sensitive credentials for the **AI Engine Gateway** using the .NET Secret Manager.

---

###  Setup Instructions

Follow these steps in order to ensure your local environment is properly configured.

#### 1. Navigate to Project Root
Open your terminal or CMD and change the directory to the gateway project folder:
```bash
cd AIEngineGateway/
```

#### 2. Initialize User Secrets
Enable secret storage for this project. This creates a unique `UserSecretsId` in your `.csproj` file.
```bash
dotnet user-secrets init
```

---

###  Configuring the JWT Key

The **JSON Web Token (JWT)** key is used to sign and verify authentication tokens. Use the following syntax to set your local secret.

| Requirement | Command Syntax |
| :--- | :--- |
| **Pattern** | `dotnet user-secrets set "Jwt:Key" "YOUR_SECRET_STRING"` |
| **Action** | Run the command below to apply the actual key: |

#### Run this command:
```bash
dotnet user-secrets set "Jwt:Key" "q8V5w2mLZ9xP3cT1uF7aK0eYbD4RjHn6sWQp2A+gMkc="
```

---

###  Important Notes
* **Local Only:** User secrets are stored in a JSON file in your system's user profile folder. They are **not** checked into source control (Git).
* **Verification:** To view your currently set secrets, you can run:
    `dotnet user-secrets list`
* **Security:** Never share the actual Base64 key string in public repositories or unencrypted chats.

> [!TIP]
> Using `dotnet user-secrets` is the recommended way to prevent sensitive keys from being accidentally leaked via `appsettings.json`.