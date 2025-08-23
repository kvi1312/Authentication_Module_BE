# Authentication Module Backend

A production-ready .NET 8.0 authentication system with Clean Architecture, JWT tokens, and dynamic configuration.

## �️ Frontend UI Available

For a complete authentication experience with user interface, clone the frontend repository:

```bash
git clone https://github.com/kvi1312/Authentication_Module_FE.git
```

The frontend provides a React-based UI for all authentication features including login, registration, token management, and admin controls.

## �🚀 Features

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

## 📁 Project Structure

```
Authentication_Module_BE/
├── README.md
└── Authentication/
    ├── Authentication.sln
    ├── docker-compose.yml
    │
    ├── Authentication.API/              # 🌐 Web API Layer
    │   ├── Program.cs
    │   ├── Endpoints/                   # Controllers
    │   ├── Extensions/                  # Service configuration
    │   └── Middleware/                  # Custom middleware
    │
    ├── Authentication.Application/      # 🎯 Business Logic
    │   ├── Commands/                    # CQRS Commands
    │   ├── Handlers/                    # Command handlers
    │   ├── Dtos/                        # Data transfer objects
    │   ├── Strategies/                  # Auth strategies
    │   └── Validators/                  # Input validation
    │
    ├── Authentication.Domain/           # 🏛️ Core Domain
    │   ├── Entities/                    # Domain entities
    │   ├── Enums/                       # Domain enums
    │   ├── Constants/                   # Domain constants
    │   └── Interfaces/                  # Domain contracts
    │
    ├── Authentication.Infrastructure/   # 🏗️ Data & External
    │   ├── AppDbContext.cs              # EF DbContext
    │   ├── Repositories/                # Data repositories
    │   ├── Services/                    # External services
    │   └── Persistence/                 # Database migrations
    │
    └── Authentication.Tests/            # 🧪 Test Suite
        ├── UnitTests/                   # Unit tests
        ├── IntegrationTests/            # Integration tests
        └── Handlers/                    # Handler tests
```

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- Docker & Docker Compose
- PostgreSQL 12+ (if not using Docker)

### Easy Setup with Docker

```bash
# Clone the repository
git clone https://github.com/kvi1312/Authentication_Module_BE.git
cd Authentication_Module_BE/Authentication

# Start all services with Docker Compose
docker-compose up -d
```

This will automatically:

- Start PostgreSQL database
- Run database migrations
- Launch the API on `https://localhost:7070`
- Set up all required services

**Access Points after Docker setup:**

- API: `https://localhost:7070`
- Swagger UI: `https://localhost:7070/swagger`
- Database: `localhost:5433` (postgres/postgres)

**Docker Commands:**

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down

# Rebuild and restart
docker-compose down && docker-compose up --build -d
```

### Manual Setup

```bash
# Clone & navigate
git clone https://github.com/kvi1312/Authentication_Module_BE.git
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

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test Authentication.Tests

# Run specific test category
dotnet test Authentication.Tests --filter "Category=Integration"
dotnet test Authentication.Tests --filter "Category=Unit"
```

### Integration Test Cases

The project includes comprehensive integration tests covering all authentication flows:

#### 🔐 Authentication Flow Integration Tests

- `RegisterUser_Should_ReturnSuccess` - User registration with validation
- `LoginUser_Should_ReturnAccessToken_And_SetCookies` - Standard login flow
- `LoginWithRememberMe_Should_SetPersistentCookies` - Remember me functionality
- `RefreshToken_Should_ReturnNewAccessToken` - Token refresh mechanism
- `Logout_Should_ClearCookies_And_InvalidateTokens` - Clean logout process
- `CompleteAuthenticationFlow_Should_WorkEndToEnd` - Full authentication cycle

#### ⏰ Token Expiration Integration Tests

- `AccessToken_Should_BeValidatedByAuthenticatedEndpoints` - Token validation
- `InvalidAccessToken_Should_Return401` - Invalid token handling
- `RefreshToken_Should_FailWithInvalidToken` - Invalid refresh token rejection
- `RefreshToken_Should_FailAfterLogout` - Post-logout token invalidation
- `RememberMeToken_Should_AllowLongerSessionDuration` - Extended session testing
- `RefreshTokenRotation_Should_ProvideNewTokens` - Token rotation security
- `UsedRefreshToken_Should_BeInvalidForSubsequentRequests` - Token reuse prevention
- `TokenExpiration_Should_RequireReauthentication` - Expired token handling

#### 👨‍💼 Admin Role Management Integration Tests

- `AdminAddRoleToUser_Should_RequireAdminAuthentication` - Admin permission validation
- `NonAdminUser_Should_NotAccessAdminEndpoints` - Access control testing
- `UserRoleValidation_Should_WorkWithDifferentRoleTypes` - Multi-role support
- `MultipleRoleAssignment_Should_HandleRoleHierarchy` - Role hierarchy management
- `BulkRoleAssignment_Should_HandleMultipleUsers` - Bulk operations testing
- `RoleRemoval_Should_RequireProperAuthorization` - Role removal security

#### 🔒 Security & Validation Integration Tests

- `AuthenticationRequest_Models_Should_SerializeCorrectly` - Request validation
- `AuthenticationResponse_Models_Should_DeserializeCorrectly` - Response validation
- `TokenExpiration_Scenarios_Should_ValidateCorrectly` - Expiration scenarios
- `RoleManagement_Scenarios_Should_ValidateCorrectly` - Role management validation
- `AuthenticationFlow_EndToEnd_Should_ValidateCorrectly` - End-to-end validation
- `HttpStatusCodes_Should_BeHandledCorrectly` - HTTP status code testing
- `CookieHandling_Should_ValidateCorrectly` - Cookie security testing
- `SecurityHeaders_Should_BeValidated` - Security headers validation

**Total Integration Tests**: 43 test cases covering all authentication scenarios

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
  - Add sending email function when user regist successfully

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
