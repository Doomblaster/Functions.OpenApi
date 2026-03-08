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

### Test Suite for Multi-Version Refactoring (2026-03-08)
- Wrote 51 new tests across 4 new files + 4 new tests in existing UnitTest1.cs (55 total new tests, all passing)
- Test structure: factory tests, 3.0 builder unit tests, 3.1 builder unit tests, 3.1 integration tests, backward-compat tests
- Key discovery: Microsoft.OpenApi 3.3.1 serializes OpenAPI 3.1 as "3.1.2" (not "3.1.0") — tests use StartsWith for resilience
- Key discovery: Amos's cache normalization fix changed schema ordering; pre-existing Test1 needs Amos to update expected JSON
- Learned xUnit v3 MTP v2 filter syntax: use `--filter-class` and `--filter-method` (not `--filter` or `--treenode-filter`)
- Decision: new tests assert JSON structure via JsonNode rather than byte-matching, making them resilient to schema ordering changes

### Cross-Agent Learning from Amos (Library Developer)
- **Cache Key Normalization Decision:** Conservative approach implemented. `GetCacheKey()` helper exists in base class but NOT applied in `BuildSchemaFromType` or `BuildSchemaFromPropertyInfo` because aggressive normalization (Nullable<T> → T) changes schema IDs (e.g., `System.Nullable_System.Guid` becomes `System.Guid`), breaking existing JSON output. Helper available for future optimization if deduplication is needed without changing identity. Bobbie's resilient test assertions (structure-based, not byte-matching) are aligned with this design.
- **FlushToDocument Change:** Changed from `Add()` to `TryAdd()` to prevent duplicate key exceptions. No impact to test logic; all builder tests pass.
- **InternalsVisibleTo Addition:** Added to csproj for test access. Enabled direct builder instantiation in Bobbie's factory and builder unit tests.
- **Build & Compatibility:** All 56 tests pass (Bobbie's 55 new + pre-existing). Default `SpecVersion = OpenApi3_0` ensures backward compatibility. Zero breaking changes.

### Repository Cleanup — Test File Renaming (2026-03-08)
- Amos renamed test files as part of overall cleanup: `UnitTest1.cs` → `OpenApiDocumentBuilderTests.cs`, test fixture `Function1.cs` → `TestFunctions.cs`
- Updated all test reference files: operationIds in expected JSON changed from `Function1Get*`/`Function1Post*` → `TestFunctionsGet*`/`TestFunctionsPost*`
- Test suite remains 56 tests; all passing. No behavioral changes from renaming.
- Bobbie's comprehensive test suite (55 new builder-specific tests) is resilient to schema ordering and naming changes via structure-based assertions (not byte-matching).

### Bug Fix Tests — Verifying Amos's 6 Bug Fixes (2026-03-08)
- **Task:** Wrote 9 tests for 6 bugs Amos is fixing; also fixed dead code in OpenApiDocumentBuilderTests.cs
- **Dead Code Fix:** Lines 313-314 had unused `JsonNode.Parse()` results; added null assertions to properly use the parsed nodes
- **Bug 1 (Dictionary Detection):** 3 tests verify `Dictionary<string,T>` and `IDictionary<string,T>` both generate `additionalProperties` schemas (OpenAPI 3.0 & 3.1)
- **Bug 2 (Primitive Schema IDs):** 2 tests verify `double` property references match schema IDs correctly (OpenAPI 3.0 & 3.1) 
- **Bug 3 (Static Functions):** 1 test verifies `BindingFlags.Static` is included, allowing static `[Function]` methods to be discovered (already fixed by Amos!)
- **Bug 4 (Singleton/Transient):** 1 test verifies building two documents produces independent results (no accumulated state)
- **Bug 5 (Culture-Sensitive ToLower):** 1 test verifies route generation uses consistent casing format
- **Bug 6 (Missing AttributeUsage):** 1 test verifies `OpenApiRequestBodyAttribute` has `[AttributeUsage]` via reflection
- **Test Results:** All 65 tests pass (56 existing + 9 new); Bug 3 already fixed, others likely fixed by Amos
- **Learning:** When testing assembly-level behavior, avoid creating actual test fixtures that interfere with existing integration tests. Use lower-level reflection tests or mock types without valid triggers.
- **Learning:** `OpenApiSchemaReference.Reference.Id` is the correct way to get reference IDs from the Microsoft.OpenApi library (not `ReferenceId`)

