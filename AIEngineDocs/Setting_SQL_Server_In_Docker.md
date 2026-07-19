# SQL Server Docker Setup on Windows 11

## Prerequisites

Before starting, ensure:

- Windows 11 is installed
- Virtualization is enabled in BIOS
- Internet connection is available
- Administrator access is available

---

# Step 1: Install Docker Desktop

## Download Docker Desktop

Download Docker Desktop from:

https://www.docker.com/products/docker-desktop/

---

## Install Docker Desktop

1. Run the installer
2. Keep default settings
3. Finish installation
4. Restart the PC if prompted

---

## Start Docker Desktop

1. Open Docker Desktop
2. Wait until Docker starts successfully

You should see:

```text
Docker Engine running
```

---

# Step 2: Verify Docker Installation

Open:

- PowerShell
or
- Windows Terminal

Run:

```bash
docker --version
```

Expected output:

```text
Docker version 28.x.x
```

---

# Step 3: Download SQL Server Docker Image

Run the following command:

```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
```

This downloads the official Microsoft SQL Server image.

---

# Step 4: Create SQL Server Container

Run the following command in PowerShell:

```bash
docker run -e "ACCEPT_EULA=Y" ^
-e "MSSQL_SA_PASSWORD=MySecurePass123!" ^
-p 1433:1433 ^
--name sqlserver ^
-v sqlvolume:/var/opt/mssql ^
-d mcr.microsoft.com/mssql/server:2022-latest
```

---

# Step 5: Understand the Docker Command

| Command Part | Description |
|---|---|
| ACCEPT_EULA=Y | Accepts Microsoft license agreement |
| MSSQL_SA_PASSWORD | Password for SQL Server admin user |
| -p 1433:1433 | Maps SQL Server port |
| --name sqlserver | Container name |
| -v sqlvolume:/var/opt/mssql | Persists database data |
| -d | Runs container in background |

---

# Step 6: Verify SQL Server Container

Run:

```bash
docker ps
```

Expected result:

```text
sqlserver
```

Status should display:

```text
Up xx seconds
```

---

# Step 7: Check SQL Server Logs

Run:

```bash
docker logs sqlserver
```

Wait until you see:

```text
SQL Server is now ready for client connections
```

This confirms SQL Server is running successfully.

---

# Step 8: Install SQL Server Management Studio (SSMS)

Download SSMS from:

https://learn.microsoft.com/en-us/ssms/download-sql-server-management-studio-ssms

Install using default settings.

---

# Step 9: Connect to SQL Server

## Open SSMS

Use the following connection details:

| Field | Value |
|---|---|
| Server Name | localhost |
| Authentication | SQL Server Authentication |
| Login | sa |
| Password | MySecurePass123! |

Click:

```text
Connect
```

---

# Useful Docker Commands

## Start SQL Server Container

```bash
docker start sqlserver
```

---

## Stop SQL Server Container

```bash
docker stop sqlserver
```

---

## Restart SQL Server Container

```bash
docker restart sqlserver
```

---

## Remove SQL Server Container

```bash
docker rm -f sqlserver
```

---

# Important Notes

## Password Requirements

SQL Server passwords must contain:

- Uppercase letter
- Lowercase letter
- Number
- Special character
- Minimum 8 characters

Example:

```text
MySecurePass123!
```

---

## Persistent Storage

This setup uses:

```text
-v sqlvolume:/var/opt/mssql
```

This ensures database data is not lost when the container restarts.

---

# Final Result

You now have:

- SQL Server running inside Docker
- Persistent database storage
- SSMS connected successfully
- ASP.NET Core EF Core integration working

Your local SQL Server is ready for development.