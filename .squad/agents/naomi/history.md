## Core Context

**Project:** Functions.OpenApi — a C# library for generating OpenAPI specifications from Azure Functions  
**Owner:** Espen  
**Stack:** .NET 10, C#, Azure Functions (isolated worker model), Microsoft.OpenApi 3.3.1, xUnit v3  
**Created:** 2026-03-08  

**Library capabilities:** Generates OpenAPI 3.0.4 (with 3.1 support in progress) specs from `[Function]` and `[HttpTrigger]` attributes; custom attributes for responses, request bodies, headers; schema builder handles primitives, collections, complex types, nullable, enums, nested objects; endpoints `/api/openapi.json` and `/api/openapi` (Scalar UI); DI integration via `services.AddOpenApi()`.

**Architecture decisions (2026-03-08):**
- **Repository restructuring:** src/, tests/, samples/ layout (standard .NET convention, enables multi-version support)
- **Multi-version OpenAPI:** Factory pattern with `IOpenApiSchemaBuilder` interface; `OpenApi30SchemaBuilder` and `OpenApi31SchemaBuilder` implementations; `OpenApiDocumentOptions.SpecVersion` property (default: 3.0)
- **Cache mechanism:** Dictionary-based with conservative key normalization (avoids nullable type collisions); prevents duplicate schema generation

**Early decisions implemented (2026-03-08):**
- Repository restructured to src/tests/samples layout (Amos, Bobbie)
- Default names removed from codebase (Amos)
- Library refactored for multi-version support (Amos, Bobbie testing)
- 14 new unit tests added for byte/byte[] schema mappings (Bobbie)
- CI pipeline + NuGet packaging configured with MinVer (tag-driven semver)
- v0.1.0 tag created successfully

### PR #1 Review Closure (2026-03-08T18:05Z)
- **Task:** Resolve all remaining PR review conversations
- **Status:** ✅ **All 13 threads resolved**
- **All actionable fixes applied by Amos** (see Amos history for details)
- **PR #1 ready for merge** — no unresolved conversations remain
- **Verification:** Build ✅ 65/65 tests pass, no regressions
- **Orchestration log:** `.squad/orchestration-log/2026-03-08T18-05-naomi.md`

### PR #1 Squash Merge into main (2026-03-08T18:58Z)
- **Task:** Squash-merge PR #1 into main branch
- **Status:** ✅ **Merge completed successfully**
- **Commit SHA:** 1e1ae90
- **Source branch:** Deleted post-merge
- **Verification:** All 65 tests pass, backward compatibility maintained
- **Orchestration log:** `.squad/orchestration-log/2026-03-08T185800Z-naomi.md`

## Learnings

### Repository Restructuring Decision (2026-03-08)
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

### PR #1 Review Closure (2026-03-08T18:05Z)
- **Task:** Resolve all remaining PR review conversations
- **Status:** ✅ **All 13 threads resolved**
- **All actionable fixes applied by Amos** (see Amos history for details)
- **PR #1 ready for merge** — no unresolved conversations remain
- **Verification:** Build ✅ 65/65 tests pass, no regressions
- **Orchestration log:** `.squad/orchestration-log/2026-03-08T18-05-naomi.md`

### Byte Type Schema Mapping Fix (2026-03-09)

**Context:** PR reviewer flagged incorrect `byte`/`byte[]` OpenAPI schema mappings.

**Finding:** Both `System.Byte` and `System.Byte[]` were combined in a single switch case producing `type: string, format: byte`. Per the OpenAPI spec, `format: byte` means base64-encoded string — correct for `byte[]` but wrong for `System.Byte` (an 8-bit unsigned integer).

**Fix applied to both `OpenApi30SchemaBuilder` and `OpenApi31SchemaBuilder`:**
- `System.Byte` → `type: integer, format: uint8` (ID: `System.Byte`)
- `System.Byte[]` → `type: string, format: byte` (ID: `System.ByteArray`)

**Additional fix:** Schema ID collision resolved — `byte[]` now uses `System.ByteArray` (matching `GetFriendlyFullName` output) instead of sharing `System.Byte`.

**Spec reference:** OpenAPI 3.0.4 §4.3 Data Types — `format: byte` is defined as "base64-encoded characters" under `type: string`. `System.Byte` is an integer, not a string.

**`GetFriendlyFullName` verification:** Already handles `byte` vs `byte[]` correctly — arrays go through the `type.IsArray` branch returning `System.ByteArray`. The bug was only in the `BuildSchemaFromType` switch case.

**Test coverage:** Bobbie wrote 14 new unit tests (7 per builder) covering byte and byte[] types in both builders. Test model (ByteTestModel.cs) includes byte, byte[], byte?, and byte[]? properties. All 81 tests pass (67 existing + 14 new).

**Verification:** Build ✅, 81/81 tests pass ✅, pushed to branch.

### CI Pipeline & NuGet Packaging Setup (2026-03-09)

**Context:** Espen requested CI pipeline for build/test on PRs and NuGet packaging on push to main.

**Decisions made:**
1. **Versioning tool: MinVer 6.0.0** — Tag-driven semver, zero config, integrates as MSBuild package. No external tool installs needed. Tags like `v1.0.0` drive version; pre-release versions auto-generated from commit height.
2. **CI workflow: `.github/workflows/ci.yml`** — Two jobs: `build-and-test` (PR gate + push) and `pack` (push to main/tags only, depends on build-and-test).
3. **global.json SDK pinning** — Added `"sdk": {"version": "10.0.103", "rollForward": "latestFeature"}` for CI reproducibility.
4. **.NET 10 preview in CI** — Uses `dotnet-version: '10.0.x'` with `dotnet-quality: 'preview'` since .NET 10 is in preview.
5. **NuGet metadata** — PackageId, Description, Authors, License (MIT), RepositoryUrl, Tags added to .csproj.
6. **README conditional** — PackageReadmeFile and README include use `Condition="Exists('..\..\README.md')"` since README doesn't exist yet.
7. **dotnet test syntax** — .NET 10 requires `--solution` flag for .slnx files, not positional argument.

**Key file paths:**
- `.github/workflows/ci.yml` — CI pipeline
- `src/Function.OpenApi/Function.OpenApi.csproj` — NuGet metadata + MinVer
- `global.json` — SDK version pinning

**Verification:** Build ✅ (0 warnings, 0 errors), 81/81 tests pass ✅, `dotnet pack` produces valid .nupkg ✅.

**Next steps for Espen:**
- Enable branch protection rule on `main` requiring `Build & Test` status check to pass before merging PRs
- Tag `v0.1.0` (or desired initial version) when ready — MinVer will pick up the version automatically
- When ready to publish to NuGet.org, add a `nuget-push` step with `NUGET_API_KEY` secret


### CI Pipeline & NuGet Packaging Setup (2026-03-09T20:40Z)
- **Task:** Set up GitHub Actions CI pipeline for NuGet packaging and PR validation
- **Status:** SUCCESS

### Branch Protection & v0.1.0 Tag (2026-03-10)
- **Branch protection:** BLOCKED — repo is private on GitHub Free plan. Both branch protection rules (`/branches/main/protection`) and repository rulesets (`/rulesets`) require GitHub Pro or a public repo. Must either upgrade plan or make repo public.
- **v0.1.0 tag:** SUCCESS — tagged and pushed. MinVer will now derive package versions from `v0.1.0` (e.g., `0.1.0-alpha.N` on branches, `0.1.0` on tagged commits).
- **Key learning:** GitHub Free private repos cannot use branch protection or rulesets via API. This is a platform limitation, not a configuration issue.
---

### Orchestration Summary (2026-03-08T21:43:30Z)
- **Task:** Document outcomes and transition to decision governance
- **Scribe action:** Orchestration log written, session log written, branch protection decision merged to decisions.md, inbox cleared, cross-agent history updated
