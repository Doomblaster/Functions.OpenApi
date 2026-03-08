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

### Repository Restructuring (2026-03-08)
- Naomi produced restructuring plan: move tests to tests/ directory (maintains same test harness, updates ProjectReference path from ..\ to ..\..\src\)
- Plan follows .NET conventions; enables multi-version support and multiple test project variants in future
- Migration uses git mv to preserve history; no test code changes needed, only path updates in .csproj and solution file
