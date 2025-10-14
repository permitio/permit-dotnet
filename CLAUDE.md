# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is the Permit.io .NET SDK - a client library for the Permit.io full-stack permissions platform. The SDK enables permission checks, user/role management, and tenant administration through both an API client and Policy Decision Point (PDP) enforcement engine.

## Development Commands

### Building the Solution
```bash
dotnet build Permit.sln
```

### Running Tests
```bash
dotnet test tests/PermitTests/PermitTests.csproj
```

### Running Example Application
```bash
dotnet run --project examples/PermitOnboardingApp/PermitOnboardingApp.csproj
```

### Code Formatting
Code formatting is handled automatically via lint-staged with csharpier:
```bash
dotnet csharpier
```

### OpenAPI Code Generation
To update the auto-generated API client code:
```bash
./generate_openapi.sh
```

This script:
- Downloads OpenAPI schemas from Permit.io APIs
- Transforms union types to simple types via Python script
- Generates C# client code using NSwag
- Fixes status code checks for 204 responses

## Architecture Overview

### Core Components

**Main SDK Class (`src/permit/Permit.cs`)**
- Entry point providing unified access to all SDK functionality
- Initializes enforcer, API client, and elements API
- Provides convenience methods for common permission checks

**Configuration (`src/permit/Config.cs`)**
- Handles SDK configuration including API tokens, URLs, and logging
- Default PDP URL: `http://localhost:7766`
- Default API URL: `https://api.permit.io`

**Permission Enforcement (`src/permit/Enforcement.cs`)**
- Core permission checking logic via PDP API calls
- Supports single checks, bulk checks, and user permission queries
- Implements both simple string-based and complex object-based checks

**API Client (`src/permit/Api.cs`)**
- Full CRUD operations for users, roles, tenants, resources
- Role assignment/unassignment functionality
- Resource instance and relationship management
- Built on top of auto-generated OpenAPI client

**Elements API (`src/permit/ElementsApi.cs`)**
- Embeddable UI components functionality
- User login delegation for embedded experiences

### Auto-Generated Code

The following files are auto-generated from OpenAPI schemas and should not be manually edited:
- `src/permit/PermitOpenAPI.cs` - Main API client
- `src/permit/PermitOpenAPIModels.cs` - API data models
- `src/permit/PermitPDPOpenAPI.cs` - PDP API client

### Project Structure

- **src/permit/** - Main SDK source code
- **src/permit/Models/** - Hand-written model classes and interfaces
- **tests/PermitTests/** - xUnit test suite with Moq for mocking
- **examples/PermitOnboardingApp/** - Example HTTP server application
- **generate_openapi.sh** - OpenAPI code generation script
- **transform_openapi.py** - OpenAPI schema transformation utility

### Key Models and Interfaces

**User Management:**
- `UserKey` - User identification (string key or complex object)
- `IUserKey` - Interface for user identification
- `ResourceInput` - Resource specification with type, tenant, attributes

**Permission Checking:**
- `CheckQuery`/`CheckQueryObj` - Permission check request structures
- `PermitCheck` - Permission check response
- `BulkPolicyDecision` - Bulk check response container

### Test Framework

- Uses xUnit testing framework
- Moq for mocking dependencies
- Test environment requires `PERMIT_API_KEY` environment variable
- Tests cover both enforcement and API functionality

### Package Dependencies

- **Microsoft.Extensions.Logging** - Structured logging
- **Newtonsoft.Json** - JSON serialization
- **NSwag.MSBuild** - OpenAPI client generation
- **System.ComponentModel.Annotations** - Data annotations

The SDK targets .NET 8.0 and .NET 9.0 frameworks.