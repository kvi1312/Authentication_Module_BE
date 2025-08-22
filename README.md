# Authentication Module Backend

A production-ready .NET 8.0 authentication system with Clean Architecture, JWT tokens, and dynamic configuration.

## 🚀 Features

- **Multi-User Authentication** (EndUser, Admin, Partner)
- **JWT Access & Refresh Tokens** with rotation
- **Secure Remember Me** with HTTP-only cookies
- **Role-Based Authorization** with dynamic assignment
- **Token Blacklisting** for immediate logout
- **Dynamic Token Configuration** (Admin runtime control)
- **Strategy Pattern** for extensible authentication

## 🛠️ Tech Stack

- **.NET 8.0** / ASP.NET Core
- **PostgreSQL** / Entity Framework Core
- **JWT Bearer** authentication
- **BCrypt** password hashing
- **Serilog** logging
- **Swagger/OpenAPI** documentation

## 🏗️ Architecture

```
API Layer       → Controllers, Middleware, Extensions
Application     → Commands, Handlers, DTOs, Strategies
Domain          → Entities, Enums, Constants
Infrastructure  → DbContext, Repositories, Services
```

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- PostgreSQL 12+

### Setup

```bash
# Clone & navigate
git clone <repo-url>
cd Authentication_Module_BE/Authentication

# Configure database in appsettings.json
{
  "DbSettings": {
    "ConnectionString": "Host=localhost;Database=AuthDB;Username=user;Password=pass"
  }
}

# Run application
cd Authentication.API
dotnet run
```

Visit: `https://localhost:7070/swagger`

## 🔐 API Endpoints

### Authentication

```http
POST /api/auth/register          # Register user
POST /api/auth/login/{userType}  # Login (1=EndUser, 2=Admin, 3=Partner)
POST /api/auth/refresh           # Refresh tokens
POST /api/auth/logout            # Logout & blacklist tokens
```

### Admin Token Configuration (SuperAdmin only)

```http
GET    /api/admin/token-config                    # Get current config
PUT    /api/admin/token-config                    # Update config
POST   /api/admin/token-config/presets/{preset}   # Apply preset
DELETE /api/admin/token-config                    # Reset to default
```

**Presets**: `very-short`, `short`, `medium`, `long`

## 🔑 Default Credentials

| User Type  | Username | Password    | Role       |
| ---------- | -------- | ----------- | ---------- |
| SuperAdmin | admin    | Admin@123   | SuperAdmin |
| EndUser    | user     | User@123    | EndUser    |
| Partner    | partner  | Partner@123 | Partner    |

## 🛡️ Security Features

- **JWT Tokens**: Configurable expiry (demo: 5min access, 6h refresh)
- **Token Rotation**: New refresh token on each use
- **Password Security**: BCrypt hashing (12 rounds)
- **Cookie Security**: HTTP-only, Secure, SameSite
- **Token Blacklisting**: Immediate invalidation

## 🧪 Example Usage

### Register User

```bash
curl -X POST "https://localhost:7070/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "email": "user@example.com",
    "password": "SecurePass@123",
    "firstName": "John",
    "lastName": "Doe",
    "userType": 1
  }'
```

### Login with Remember Me

```bash
curl -X POST "https://localhost:7070/api/auth/login/1" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "password": "SecurePass@123",
    "rememberMe": true
  }'
```

### Use Protected Endpoint

```bash
curl -X GET "https://localhost:7070/api/protected" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Admin: Apply Demo Preset

```bash
# Login as SuperAdmin first, then:
curl -X POST "https://localhost:7070/api/admin/token-config/presets/very-short" \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

## 🎭 Strategy Pattern

Extensible authentication strategies per user type:

- **AdminAuthenticationStrategy**: Admin-specific validation + claims
- **PartnerAuthenticationStrategy**: Partner-specific validation + claims
- **EndUserAuthenticationStrategy**: EndUser-specific validation + claims

## 📊 Database Schema

**Core Tables**: Users, Roles, UserRoles, RefreshTokens, RememberMeTokens, UserSessions

**Default Roles**:

- EndUser: `EndUser`, `PremiumEndUser`
- Admin: `Admin`, `SuperAdmin`, `SystemAdmin`
- Partner: `Partner`, `PartnerAdmin`, `PartnerUser`

## 🔧 Configuration

### JWT Settings (appsettings.json)

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

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add "MigrationName" --project Authentication.Infrastructure --startup-project Authentication.API --output-dir Persistence/Migrations

# Update database
dotnet ef database update --project Authentication.Infrastructure --startup-project Authentication.API

# Remove last migration
dotnet ef migrations remove --project Authentication.Infrastructure --startup-project Authentication.API
```

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## � TODO

### 🚀 Upcoming Features

- [ ] **Redis Token Management**: Implement Redis for token storage and blacklisting
  - Replace in-memory token blacklist with Redis
  - Add distributed token caching
  - Improve scalability for multiple instances
  - Add Redis health checks and failover

### 🔧 Technical Improvements

- [ ] Add comprehensive unit tests
- [ ] Implement rate limiting
- [ ] Add 2FA authentication
- [ ] OAuth2 provider integration
- [ ] Performance monitoring and metrics

## �📝 License

This project is licensed under the MIT License.

---

⭐ **Star this repo if you find it helpful!**
