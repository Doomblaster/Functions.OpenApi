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

### Multi-Version OpenAPI Refactoring Proposed (2026-03-08)
- Naomi completed architecture analysis: refactoring strategy documented with IOpenApiSchemaBuilder interface + version-specific implementations
- Refactoring plan includes comprehensive 5-phase approach: Foundation, 3.1 implementation, Integration, Testing, Future enhancements
- Cache evaluation: KEEP with nullable key normalization fix to prevent duplicate schemas and Components.Schemas collisions
- Testing scope: Comprehensive unit + integration tests for both 3.0 and 3.1, byte-for-byte JSON comparison for backward compatibility
- Estimated effort: 5–6 days total; Bobbie allocated 2 days for testing phase
- STATUS: Awaiting Espen approval before Phase 1 (foundation) execution
