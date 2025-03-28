﻿![NET.png](imgs/NET.png)
# Permit.io client for dotnet

dotnet client library for the Permit.io full-stack permissions platform.

# To update the API autogenerated code for the Permit.io API

1. Install the latest version of the `OpenAPI Generator` CLI tool from https://openapi-generator.tech/docs/installation
2. Run the following command to generate both the PDP and main API client code:
```bash
./generate_openapi.sh
```

This script will:
- Download the latest OpenAPI schema files from Permit.io
- Transform the OpenAPI union types to simple types
- Generate the client code for both the main Permit.io API and PDP API
- Fix status code checks to support 204 status codes

## Installation
```
Add nuget package to your project and use with:
using Permit
using Permit.Models
```
