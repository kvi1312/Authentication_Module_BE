# Authentication Module Backend

Enterprise-grade .NET 8.0 authentication system with JWT tokens, role-based authorization, and Clean Architecture.

## Features

- **Multi-User Authentication** - EndUser, Admin, Partner roles
- **JWT Token Management** - Access/Refresh tokens with rotation
- **Security Features** - Token blacklisting, BCrypt hashing, HTTP-only cookies
- **Dynamic Configuration** - Runtime token configuration by admin
- **Clean Architecture** - CQRS, Strategy Pattern, Dependency Injection, Repository Pattern
- **Role-Based Access Control** - Fine-grained permissions system
- **Remember Me** - Extended session with persistent tokens
- **Auto Migration & Seeding** - Database setup with default users
- **Swagger Documentation** - Interactive API testing
- **Docker Support** - Easy deployment and development

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

### Setup Source Code

```bash
# Clone repository
git clone https://github.com/kvi1312/Authentication_Module_BE.git

# Navigate to Authentication folder
cd Authentication_Module_BE/Authentication

# Start services with Docker Compose
docker-compose up -d
```

## API Endpoints

### Authentication

```http
POST /api/auth/register          # Register new user
POST /api/auth/login             # Login with username/password
POST /api/auth/refresh-token     # Refresh access token
POST /api/auth/logout            # Logout and blacklist tokens
```

### User Management

```http
GET    /api/user/profile         # Get current user profile
PUT    /api/user/profile         # Update user profile
GET    /api/user/all             # Get all users (Admin only)
GET    /api/user/{userId}        # Get user by ID (Admin only)
POST   /api/user/{userId}/roles/add     # Add role to user (Admin only)
POST   /api/user/{userId}/roles/remove  # Remove role from user (Admin only)
```

### Admin (SuperAdmin only)

```http
GET    /api/admin/token-config              # Get current token configuration
PUT    /api/admin/token-config              # Update token configuration
POST   /api/admin/token-config/reset        # Reset to default configuration
POST   /api/admin/token-config/preset/{presetName}  # Apply preset (very-short, short, medium, long)
GET    /api/admin/token-config/presets      # Get available presets
```

## Default Credentials

### Admin Users

| Role       | Username  | Password      | Description                       |
| ---------- | --------- | ------------- | --------------------------------- |
| SuperAdmin | admin     | Admin@123     | Full system administrative access |
| Admin      | moderator | Moderator@123 | Standard administrative access    |
| Manager    | manager   | Manager@123   | Management level access           |
| Employee   | employee  | Employee@123  | Employee level access             |

### End Users

| Role     | Username | Password     | Description              |
| -------- | -------- | ------------ | ------------------------ |
| Customer | customer | Customer@123 | Standard customer access |
| Guest    | guest    | Guest@123    | Guest user access        |

### Partners

| Role    | Username | Password    | Description             |
| ------- | -------- | ----------- | ----------------------- |
| Partner | partner  | Partner@123 | Standard partner access |

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

### Token Configuration Presets

| Preset     | Access Token | Refresh Token | Remember Me Token |
| ---------- | ------------ | ------------- | ----------------- |
| very-short | 2 minutes    | 30 minutes    | 2.4 hours         |
| short      | 5 minutes    | 6 hours       | 1 day             |
| medium     | 15 minutes   | 1 day         | 1 week            |
| long       | 1 hour       | 1 week        | 1 month           |

**Usage:** `POST /api/admin/token-config/preset/{presetName}`

### Database Migration

```bash
# Add migration
dotnet ef migrations add "migration_name" --project Authentication.Infrastructure --startup-project Authentication.API --output-dir Persistence/Migrations

# Update database
dotnet ef database update --project Authentication.Infrastructure --startup-project Authentication.API
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

MIT License - Free to use, modify, and distribute
