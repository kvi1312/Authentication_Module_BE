# Authentication Module Backend

Enterprise-grade .NET 8.0 authentication system with JWT tokens, role-based authorization, and Clean Architecture.

## Features

- **Multi-User Authentication** - EndUser, Admin, Partner roles
- **JWT Token Management** - Access/Refresh tokens with rotation
- **Security Features** - Token blacklisting, BCrypt hashing, HTTP-only cookies
- **Dynamic Configuration** - Runtime token configuration by admin
- **Clean Architecture** - CQRS, Strategy Pattern, Dependency Injection, Repository Pattern

## Tech Stack

- .NET 8.0, ASP.NET Core, Entity Framework Core
- PostgreSQL, JWT Bearer, BCrypt, Serilog
- Docker, Swagger/OpenAPI

## Architecture

```
├── API Layer          # Controllers, Middleware
├── Application Layer  # Commands, Handlers, DTOs
├── Domain Layer       # Entities, Business Logic
└── Infrastructure     # Database, External Services
```

## Quick Start

### Using Docker (Recommended)

```bash
git clone https://github.com/kvi1312/Authentication_Module_BE.git
cd Authentication_Module_BE/Authentication
docker-compose up -d
```

**Access Points:**

- API: `https://localhost:7070`
- Swagger: `https://localhost:7070/swagger`
- Database: `localhost:5433`

### Manual Setup

```bash
cd Authentication.API
dotnet run
```

## API Endpoints

### Authentication

```http
POST /api/auth/register          # Register user
POST /api/auth/login/{userType}  # Login (1=EndUser, 2=Admin, 3=Partner)
POST /api/auth/refresh           # Refresh tokens
POST /api/auth/logout            # Logout
```

### Admin (SuperAdmin only)

```http
GET    /api/admin/token-config   # Get configuration
PUT    /api/admin/token-config   # Update configuration
POST   /api/admin/token-config/presets/{preset}   # Apply preset
```

## Default Credentials

| Role       | Username | Password    |
| ---------- | -------- | ----------- |
| SuperAdmin | admin    | Admin@123   |
| EndUser    | user     | User@123    |
| Partner    | partner  | Partner@123 |

## Configuration

### JWT Settings

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-32-chars-min",
    "ExpiryMinutes": 5,
    "RefreshTokenExpiryDays": 0.25,
    "RememberMeTokenExpiryDays": 1
  }
}
```

### Database Migration

```bash
# Add migration
dotnet ef migrations add "MigrationName" --project Authentication.Infrastructure

# Update database
dotnet ef database update --project Authentication.Infrastructure
```

## Frontend UI

React-based frontend available at:

```bash
git clone https://github.com/kvi1312/Authentication_Module_FE.git
```

## Contributing

1. Fork → Create branch → Commit → Push → Pull Request
2. Follow Clean Architecture principles
3. Include tests for new features

## License

MIT License - See LICENSE file for details
