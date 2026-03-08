# Project Context

- **Owner:** Espen
- **Project:** Functions.OpenApi — a C# library for generating OpenAPI specifications from Azure Functions
- **Stack:** .NET 10, C#, Azure Functions (isolated worker model), Microsoft.OpenApi 3.3.1, xUnit v3
- **Created:** 2026-03-08

## Key Facts

- Library generates OpenAPI 3.0.4 specs from `[Function]` and `[HttpTrigger]` attributes
- Custom attributes: `[OpenApiResponse]`, `[OpenApiRequestBody]`, `[OpenApiHeader]`
- Schema builder handles primitives, collections, complex types, nullable, enums, nested objects
- Builders: OpenApiDocumentBuilder (main + Paths, Responses, Parameters partials), OpenApiSchemaBuilder
- DI integration via ConfigurationExtensions.AddOpenApi()
- Goal: extend support to OpenAPI 3.1 and newer versions

## Learnings

### Repository Restructuring (2026-03-08)
- Naomi produced comprehensive restructuring plan (src/tests/samples layout) per team decision to organize codebase per .NET conventions
- Plan includes: current state, proposed structure, file-by-file path updates, step-by-step migration using git mv, risk assessment, post-migration checklist
- Tests remain coupled to main library via ProjectReference; restructuring updates paths from ..\ to ..\..\src\
- Low execution risk; git history preserved; main breakage is IDE/contributor workflow (mitigated by communication and clear steps)

### Restructuring Executed (2026-03-08)
- Moved Function.OpenApi → src/Function.OpenApi, Function.OpenApi.Tests → tests/Function.OpenApi.Tests, FunctionApp1 → samples/FunctionApp1
- Updated Function.OpenApi.slnx with new project paths
- Updated ProjectReference in both tests and samples csproj files (..\ → ..\..\src\)
- git mv had issues with pre-created target directories on Windows; workaround: rename to temp name first, then move into target
- No other files (CI workflows, configs) needed path updates — only .squad decision docs reference old paths (historical, no action needed)
- Build succeeded, all tests passed (1/1), committed with git history preserved via renames

### Multi-Version OpenAPI Refactoring Proposed (2026-03-08)
- Naomi completed comprehensive architecture analysis and produced 14-step refactoring plan
- Recommendation: IOpenApiSchemaBuilder interface + version-specific implementations (OpenApi30SchemaBuilder, OpenApi31SchemaBuilder)
- Factory pattern for spec version configuration; default SpecVersion = OpenApi3_0 maintains backward compatibility
- Cache verdict: Keep with nullable key normalization fix (prevents duplicate schemas, avoids Components.Schemas collisions)
- Estimated effort: 5–6 days (Amos 3–4 days implementation, Bobbie 2 days testing)
- STATUS: Awaiting Espen approval before Phase 1 execution
