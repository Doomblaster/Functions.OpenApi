# Project Context

- **Owner:** Espen
- **Project:** Functions.OpenApi — a C# library for generating OpenAPI specifications from Azure Functions
- **Stack:** .NET 10, C#, Azure Functions (isolated worker model), Microsoft.OpenApi 3.3.1, xUnit v3
- **Created:** 2026-03-08

## Key Facts

- FunctionApp1 is the sample app demonstrating library usage
- DI setup: services.AddOpenApi(options => { Title, Version, ServerUrls, Assemblies })
- Endpoints: /api/openapi.json (spec) and /api/openapi (Scalar UI)
- Sample app currently minimal — basic function with no OpenAPI attributes
- Goal: extend support to OpenAPI 3.1 and newer versions

## Learnings

### Repository Restructuring (2026-03-08)
- Naomi produced restructuring plan: move FunctionApp1 sample to samples/ directory (maintains same structure and functionality, updates ProjectReference path from ..\ to ..\..\src\)
- Samples separated from tests in new layout; enables multiple demo/sample applications without confusion
- Plan follows .NET conventions; uses git mv for history preservation; no code changes needed, only path updates in .csproj and solution file
