# Project Context

- **Owner:** Espen
- **Project:** Functions.OpenApi — a C# library for generating OpenAPI specifications from Azure Functions
- **Stack:** .NET 10, C#, Azure Functions (isolated worker model), Microsoft.OpenApi 3.3.1, xUnit v3
- **Created:** 2026-03-08

## Key Facts

- Library generates OpenAPI 3.0.4 specs from `[Function]` and `[HttpTrigger]` attributes
- Custom attributes: `[OpenApiResponse]`, `[OpenApiRequestBody]`, `[OpenApiHeader]`
- Schema builder handles primitives, collections, complex types, nullable, enums, nested objects
- Endpoints: `/api/openapi.json` (spec) and `/api/openapi` (Scalar UI)
- DI integration: `services.AddOpenApi(options => {...})`
- Goal: extend support to OpenAPI 3.1 and newer versions

## Learnings

### Restructuring Decision (2026-03-08)
- **Repository layout:** Currently flat (Function.OpenApi/, Function.OpenApi.Tests/, FunctionApp1/ at root). Proposed: src/, tests/, samples/
- **Project structure:** 3 projects total — main library, xUnit v3 tests (Microsoft.Testing.Platform runner), sample Azure Functions app (isolated worker)
- **Project references:** Tests and samples both reference main library via relative paths (..\ from peer directories). After restructuring, will use ..\..\src\ paths.
- **Config files:** Solution file (.slnx) contains 3 project entries with relative paths; global.json and nuget.config need no changes
- **CI/CD:** Existing .github/workflows are squad infrastructure (heartbeat, triage, labels) — no project-specific path references. Future CI workflows will need path updates.
- **Rationale:** Standard .NET convention; enables clear separation of library, tests, samples; future-proofs for multi-version support (3.0, 3.1) where version-specific builders/projects may be isolated into src/
- **Risk profile:** Low; git history preserved with git mv; breaking change is IDE/contributor workflow (mitigated by communication and clear steps)
- **Post-migration capability:** Structure enables variant libraries, integration tests, and multiple sample apps without confusion
- **Status:** Decision recorded in .squad/decisions.md (2026-03-08T13:13Z). Awaiting team approval before execution.

### Multi-Version OpenAPI Architecture (2026-03-08)

**Current Architecture Analysis:**
- **Core builders:** `OpenApiDocumentBuilder` (partial class, 4 files) orchestrates document generation; `OpenApiSchemaBuilder` handles type-to-schema conversion
- **Schema generation flow:** Scans Azure Functions methods → builds schemas from types → caches in Dictionary → flushes to Components.Schemas
- **OpenAPI 3.0 hardcoding:** Spec version hardcoded in `OpenApiJsonEndpoint.cs:23` and tests; nullable handling uses `JsonSchemaType.Null | otherType` with `"nullable": true` extension (3.0 pattern)
- **Cache mechanism:** `Dictionary<(Type, bool Nullable), OpenApiSchema>` in `OpenApiSchemaBuilder` prevents duplicate schema generation and Components.Schemas key collisions

**OpenAPI 3.0 vs 3.1 Key Differences:**
1. **Nullable handling (BREAKING):** 3.0 uses `"nullable": true` extension; 3.1 uses JSON Schema native `"type": ["string", "null"]` arrays
2. **JSON Schema alignment:** 3.0 based on draft-04; 3.1 fully aligned with JSON Schema 2020-12
3. **Example keyword:** 3.0 uses `"example"` (singular); 3.1 uses `"examples"` (array)
4. **Schema references:** 3.0 only allows `$ref`; 3.1 allows combining `$ref` with other properties
5. **Microsoft.OpenApi handling:** The library automatically serializes nullable types correctly based on `OpenApiSpecVersion` passed to serialization; our schema construction doesn't need to change for nullable *if* we set the Type flag correctly

**Proposed Refactoring Architecture:**
- **Abstraction:** Introduce `IOpenApiSchemaBuilder` interface with `SpecVersion`, `BuildComponentSchema()`, `BuildSchemaFromPropertyInfo()`, `FlushToDocument()`
- **Base class:** `OpenApiSchemaBuilderBase` provides common logic (caching, type name generation, nullability checking, collection detection)
- **Version-specific implementations:** `OpenApi30SchemaBuilder` (current behavior) and `OpenApi31SchemaBuilder` (Examples array, JSON Schema 2020-12 semantics)
- **Factory pattern:** `OpenApiSchemaBuilderFactory.Create(document, specVersion)` returns correct builder
- **Configuration:** Add `OpenApiDocumentOptions.SpecVersion` property (default: `OpenApi3_0` for backward compatibility)
- **Endpoint update:** `OpenApiJsonEndpoint` uses configured spec version instead of hardcoded value

**Cache Analysis Verdict: KEEP with improvements**
- **Problem solved:** Prevents duplicate schema generation, avoids Components.Schemas key collisions, improves performance via type reflection caching
- **Cache necessity:** Yes — without it, multiple properties of same type would create duplicate schemas and cause Dictionary.Add() collisions
- **Thread safety issue (MEDIUM):** Dictionary not thread-safe; currently low risk (per-request builder), future risk if concurrent builds needed; recommend documenting limitation or using ConcurrentDictionary
- **Nullable cache key bug (MEDIUM):** Current implementation can cache `Guid?` with two different keys: `(typeof(Guid?), true)` from BuildSchemaFromPropertyInfo and `(typeof(Guid?), false)` from BuildComponentSchema; solution: normalize cache key to use underlying type + explicit nullable flag
- **Memory retention (LOW):** Cache scoped to single BuildDocument() call, cleared on GC; acceptable for typical usage
- **Fix required:** Implement `GetCacheKey(Type, bool)` helper to normalize nullable value types to their underlying type before caching

**Backward Compatibility Strategy:**
- Default `SpecVersion = OpenApi3_0` ensures zero breaking changes for existing consumers
- All existing tests pass without modification
- New functionality (3.1 support) is opt-in via configuration

**Refactoring Phases:**
1. **Foundation (low risk):** Create interface, base class, refactor existing builder to `OpenApi30SchemaBuilder`
2. **OpenAPI 3.1 (medium risk):** Implement `OpenApi31SchemaBuilder` with Examples array and JSON Schema 2020-12 semantics
3. **Integration (medium risk):** Wire up factory, add SpecVersion to options, update endpoint serialization
4. **Testing (low risk):** Comprehensive unit + integration tests for both 3.0 and 3.1
5. **Future (optional):** Thread-safe cache, schema $id support, contentSchema in 3.1

**Risk Assessment:**
- **High-risk areas:** Factory integration in InitializeDocument(), cache key normalization, endpoint spec version plumbing
- **Mitigation:** Comprehensive testing at each step, byte-for-byte JSON comparison for 3.0 output, integration tests for 3.1
- **Rollback plan:** Revert to original OpenApiSchemaBuilder if Phase 1-2 issues; keep refactored builders but revert integration if Phase 3 issues

**Extensibility for Future Versions:**
- Add new `OpenApiXYSchemaBuilder` inheriting from base class
- Update factory switch statement
- No changes to core document builder or configuration (just new enum value)

**Decision Status:** Proposed, awaiting Espen approval. Estimated effort: 5-6 days (Amos 3-4 days implementation, Bobbie 2 days testing).

### PR #1 Review (2026-03-08)

**Context:** Reviewed 13 automated review comments from `copilot-pull-request-reviewer` on PR #1 (feat: Repository restructuring and multi-version OpenAPI support).

**Key findings verified and triaged:**
- **Dictionary handling bug (3.0 + 3.1):** Confirmed — only `IDictionary<,>` interface matched, concrete `Dictionary<K,V>` falls through to object reflection. Real bug, needs fix.
- **Singleton registration of stateful builder:** Confirmed — `OpenApiDocumentBuilder` is registered as singleton but mutates `_document` and `_schemaBuilder` per call. Race condition under concurrent requests.
- **Static function methods not scanned:** Confirmed — `BindingFlags.Public | BindingFlags.Instance` excludes static Azure Functions. Real gap.
- **Primitive schema ID mismatch:** Confirmed — IDs like "double", "byte", "bool" don't match `GetFriendlyFullName()` output. Could break `$ref` resolution.
- **Culture-sensitive `ToLower()`:** Confirmed — Turkish-I problem applies. Quick fix.
- **Missing `[AttributeUsage]` on `OpenApiRequestBodyAttribute`:** Confirmed — other attributes have it, this one doesn't. Consistency issue.
- **Hardcoded Scalar UI URL:** Confirmed — `/api/openapi.json` hardcoded. Breaks with custom route prefixes.
- **Config files with machine-specific paths:** Confirmed — `.squad/config.json`, `.copilot/config.json`, `.copilot/mcp-config.json` contain local paths/user data.
- **Unused JSON parsing in test:** Confirmed — dead code in assertion.

**Decision:** 6 comments are actionable bugs (Amos), 3 are config hygiene (Espen decision), 1 is test cleanup (Bobbie), 3 are duplicates of the dictionary/schema-ID issues across 3.0/3.1 builders.

