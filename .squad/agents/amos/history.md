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

### Multi-Version OpenAPI Refactoring Implemented (2026-03-08)
- Executed Naomi's approved refactoring plan (Phases 1–3)
- **Phase 1 (Foundation):** Created `IOpenApiSchemaBuilder` interface, `OpenApiSchemaBuilderBase` abstract class (shared logic, caching, `FlushToDocument`, static helpers like `GetFriendlyFullName`, `IsPropertyNullable`, `IsCollectionType`), renamed current builder to `OpenApi30SchemaBuilder`, created `OpenApiSchemaBuilderFactory`
- **Phase 2 (3.1 Support):** Created `OpenApi31SchemaBuilder` — main difference is `Examples` (array) instead of `Example` (singular) for `DateOnly`, and propagates `Examples` in nullable handling. Factory updated to support both versions.
- **Phase 3 (Configuration & Integration):** Added `SpecVersion` property to `OpenApiDocumentOptions` (default `OpenApi3_0`), updated `OpenApiDocumentBuilder.InitializeDocument()` to use factory, updated `OpenApiJsonEndpoint` to inject `OpenApiDocumentOptions` and use configured `SpecVersion` instead of hardcoded `OpenApi3_0`, registered `OpenApiJsonEndpoint` and `OpenApiUIEndpoint` explicitly in `ConfigurationExtensions`
- **Cache normalization decision:** `GetCacheKey()` helper exists in base class but aggressive normalization (`Nullable<T>` → `T`) was NOT applied in `BuildSchemaFromType` or `BuildSchemaFromPropertyInfo` because it changes schema IDs (e.g., `System.Nullable_System.Guid` becomes `System.Guid`), breaking existing JSON output. The helper is available for future use if cache deduplication is needed without changing schema identity.
- **Key file paths:**
  - `src/Function.OpenApi/Builders/IOpenApiSchemaBuilder.cs` — interface
  - `src/Function.OpenApi/Builders/OpenApiSchemaBuilderBase.cs` — shared logic
  - `src/Function.OpenApi/Builders/OpenApi30SchemaBuilder.cs` — 3.0 implementation
  - `src/Function.OpenApi/Builders/OpenApi31SchemaBuilder.cs` — 3.1 implementation
  - `src/Function.OpenApi/Builders/OpenApiSchemaBuilderFactory.cs` — factory
- **InternalsVisibleTo:** Added `Function.OpenApi.Tests` to csproj for test access to internal builders
- Partial classes (`OpenApiDocumentBuilder.Responses.cs`, `.Parameters.cs`) updated to reference `OpenApiSchemaBuilderBase.GetFriendlyFullName()` instead of deleted `OpenApiSchemaBuilder`
- Old `OpenApiSchemaBuilder.cs` deleted — replaced by base class + version-specific implementations
- `FlushToDocument` changed from `Add` to `TryAdd` to prevent duplicate key exceptions
- All 56 tests pass (including pre-existing + new builder-specific tests)

### Cross-Agent Learning from Bobbie (Tester)
- **Microsoft.OpenApi Behavior:** Microsoft.OpenApi 3.3.1 serializes OpenAPI 3.1 as "3.1.2" (not "3.1.0") — tests use `StartsWith("3.1")` for resilience to patch version changes. Relevant for any future version detection logic.
- **Cache Normalization Side Effects:** Schema ordering changed due to cache deduplication approach. Pre-existing `UnitTest1.Test1` expected JSON reflects this. Bobbie's comprehensive test suite (55 new tests) validates all schema generation paths and found no regressions in generated schemas.
- **Test Coverage:** Bobbie wrote 55 new tests across factory, 3.0 builder, 3.1 builder, and integration paths. All pass. Factory instantiation, builder contract adherence, and version-specific behavior all validated.

### Repository Cleanup — Default Name Removal (2026-03-08)
- **Sample project renamed:** `samples/FunctionApp1/` → `samples/Function.OpenApi.Sample/`, `FunctionApp1.csproj` → `Function.OpenApi.Sample.csproj`, `Function1.cs` → `SampleFunctions.cs`, class `Function1` → `SampleFunctions`, namespace `FunctionApp1` → `Function.OpenApi.Sample`
- **Test fixture renamed:** `tests/.../Function1.cs` → `TestFunctions.cs`, class `Function1` → `TestFunctions`; updated all `typeof(Function1)` references and operationIds in expected JSON across 3 test files
- **Test file renamed:** `UnitTest1.cs` → `OpenApiDocumentBuilderTests.cs`, class `UnitTest1` → `OpenApiDocumentBuilderTests`, test method `Test1` → `FullDocument_SerializesExpectedJson`
- **Unused code removed:** duplicate `using System.Linq;` and unused `using System.Text.Json;`, `using System.Threading.Tasks;` from `OpenApiDocumentBuilderTests.cs`; unused `RemoveDateOnlyExample` private method; commented-out `PackageReference` for `Microsoft.Azure.Functions.Worker.Sdk` in test csproj
- **Solution file updated:** `Function.OpenApi.slnx` path updated to `samples/Function.OpenApi.Sample/Function.OpenApi.Sample.csproj`
- **Windows git mv workaround:** folder rename required temp name trick (same as restructuring) due to file locks from obj/bin
- All 56 tests pass, build succeeds with 0 errors

