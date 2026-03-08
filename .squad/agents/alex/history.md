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

<!-- Append new learnings below. Each entry is something lasting about the project. -->
