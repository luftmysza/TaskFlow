# TaskFlow

**TaskFlow** is a modular, layered .NET Core application for managing collaborative projects and tasks. It provides:

-  A RESTful API with JWT-based authentication
-  An admin panel using Razor Pages and Identity
-  A SOAP service for client interoperability
-  Role-based access control (global + per-project)
-  Automated test scripts for endpoints
-  A clean architecture approach for full separation of concerns

---

## Architecture

```
TaskFlow/
├── TaskFlow.Domain         ← Entities and core business rules
├── TaskFlow.Application    ← Interfaces, DTOs, use cases
├── TaskFlow.Infrastructure ← DB access, identity, services
├── TaskFlow.API            ← REST API (JWT-based)
├── TaskFlow.Razor          ← Admin panel (Razor Pages + Identity)
├── TaskFlow.SOAP           ← SOAP interface for comment retrieval
├── TaskFlow.SOAPClient     ← Console client consuming the SOAP endpoint
```

Follows **Clean Architecture** principles.

---

## Authentication & Authorization

### Global Roles
- `Admin` – full system access
- `User` – restricted access

### Project Roles
- `Owner` – full access to their project
- `Participant` – can view & contribute

### Auth Methods

| Application       | Auth Method         |
|------------------|---------------------|
| `API`            | JWT Bearer Tokens   |
| `Razor` (Admin)  | ASP.NET Identity    |
| `SOAP`           | JWT Bearer Tokens   |

---

## Features

### User
- Login (JWT or Identity)
- Global or project-level roles

### Projects
- Create / view / delete
- Assign users with specific roles

### Tasks
- Create, assign, comment
- Mark as completed

### Comments
- Add comments to tasks
- SOAP endpoint for retrieving comments addressed to a user

---

## Getting Started

```bash
cd TaskFlow.API
dotnet run
```

```bash
cd TaskFlow.Razor
dotnet run
```

```bash
cd TaskFlow.SOAP
dotnet run
```

Seed data is initialized unless data already exists.

---

## API Testing

Run the Windows Batch script:

```cmd
test_endpoints.bat
```

Logs are saved in `api_test_results.txt`.

---

## SOAP Testing

- Endpoint: `/CommentsSoapService.asmx`
- JWT token must be passed in `Authorization` header
- Client app: `TaskFlow.SOAPClient`

---

## Tools & Technologies

- ASP.NET Core 8
- EF Core (InMemory)
- ASP.NET Identity
- Razor Pages
- JWT Auth
- SoapCore

---

## Reset & Cleanup

Data is seeded only if the DB is empty.

---

## File Highlights

| File / Dir                  | Purpose                               |
|----------------------------|----------------------------------------|
| `test_endpoints.bat`       | Automates login + CRUD API calls       |
| `DbSeeder.cs`              | Seeds roles, users, project, task      |
| `CommentsSoapService.cs`   | SOAP contract and logic                |
| `AddInfrastructure(...)`   | Centralized service wiring             |
| `README.txt`               | You're reading it!                     |

---

## Authors

Developed by **Dzmitry Zaitsau**

---

## License

Provided for **educational and demonstration purposes**.
