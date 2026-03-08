# Project Context

- **Owner:** Espen
- **Project:** Functions.OpenApi — a C# library for generating OpenAPI specifications from Azure Functions
- **Stack:** .NET 10, C#, Azure Functions (isolated worker model), Microsoft.OpenApi 3.3.1, xUnit v3
- **Created:** 2026-03-08

## Key Facts

- Tests in Function.OpenApi.Tests using xUnit v3 with MTP v2 runner
- Test functions in Function1.cs with OpenAPI attributes
- Test models: RequestBody (Guid, string, int, DateOnly, List, enum), ResponseBody (nested objects, collections)
- Tests validate generated OpenAPI JSON structure and schema correctness
- Goal: extend support to OpenAPI 3.1 and newer versions

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->
