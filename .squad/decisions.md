# Squad Decisions

## Active Decisions

### 1. Repository Restructuring: `src/` and `tests/` Layout

**Decision ID:** naomi-repo-restructuring  
**Owner:** Naomi (Lead / Architect)  
**Date:** 2026-03-08  
**Status:** Proposed (awaiting team approval)

Restructure the repository to follow standard .NET conventions:
- **Source code** → `src/` directory
- **Tests** → `tests/` directory  
- **Samples** → `samples/` directory (separate from source, distinct from tests)

This improves organization, follows .NET community standards, and supports scalability as the project grows to support multiple spec versions (3.0, 3.1, etc.).

**Files affected:**
- Function.OpenApi.slnx (paths updated)
- Function.OpenApi.Tests/Function.OpenApi.Tests.csproj (project reference updated)
- FunctionApp1/FunctionApp1.csproj (project reference updated)
- Directories moved: Function.OpenApi/ → src/, Function.OpenApi.Tests/ → tests/, FunctionApp1/ → samples/

**Rationale:**
- Standard .NET convention; enables clear separation of library, tests, samples
- Future-proofs for multi-version support (version-specific builders/projects in src/)
- Follows conventions used by Steeltoe, NServiceBus, and similar projects
- Low risk: git history preserved with git mv; only IDE/contributor workflow affected

**Risks & Mitigations:**
- Breaking contributor workflows → communicate plan in advance; provide clear steps
- CI/CD pipeline breaks → test locally first; no project-specific CI workflows currently exist
- IDE cache issues → recommend dotnet clean or IDE reload after migration
- Documentation outdated → update setup/build docs as needed

**Next steps:** Awaiting approval from Espen (owner). Once consensus reached, execute Step 1–7 (create directories, move with git mv, update paths, verify build).

**See:** `.squad/decisions/inbox/naomi-repo-restructuring.md` for full specification and migration steps.

---

### 2. Repository Cleanup — Default Name Removal

**Decision ID:** amos-repo-cleanup  
**Owner:** Amos (Library Developer)  
**Date:** 2026-03-08  
**Status:** Implemented  
**Requested by:** Espen

Removed all default placeholder names from the repository. No behavioral changes — purely naming and dead code cleanup.

**Changes:**
- `samples/FunctionApp1/` → `samples/Function.OpenApi.Sample/`
- `FunctionApp1.csproj` → `Function.OpenApi.Sample.csproj`
- `samples/.../Function1.cs` → `SampleFunctions.cs` (class name, namespace updated)
- `tests/.../UnitTest1.cs` → `OpenApiDocumentBuilderTests.cs` (class name, method names)
- `tests/.../Function1.cs` → `TestFunctions.cs` (test fixture class)
- Removed duplicate usings, unused methods, commented-out package references
- Updated 5+ reference files (solution, Program.cs, test files with operationId expectations)

**Verification:**
- Build: ✅ 0 errors
- Tests: ✅ 56/56 pass
- No behavioral changes to library or test coverage

**See:** `.squad/orchestration-log/2026-03-08T14-06-amos.md` for detailed implementation notes.

---

### 3. Library Refactoring for Multi-Version OpenAPI Support

**Decision ID:** naomi-library-refactoring  
**Owner:** Naomi (Lead / Architect)  
**Date:** 2026-03-08  
**Status:** Implemented

**Approved Architecture:**
- `IOpenApiSchemaBuilder` interface with version-specific implementations ✅
- `OpenApiSchemaBuilderBase` for shared logic (caching, type reflection) ✅
- `OpenApi30SchemaBuilder` (current behavior) and `OpenApi31SchemaBuilder` (JSON Schema 2020-12) ✅
- Factory pattern: `OpenApiSchemaBuilderFactory.Create(document, specVersion)` ✅
- `OpenApiDocumentOptions.SpecVersion` property (default: `OpenApi3_0`) ✅
- Cache maintained with conservative key normalization approach ✅

**Implementation Summary (Amos):**
- **Phase 1 (Foundation):** Created interface, base class with shared logic, 3.0 builder, factory
- **Phase 2 (3.1 Support):** Implemented 3.1 builder with Examples array for DateOnly
- **Phase 3 (Integration):** Added SpecVersion configuration, integrated factory, updated endpoints
- **Cache Decision:** Conservative approach — `GetCacheKey()` helper exists but NOT applied to preserve schema identity and backward compatibility
- **Test Fixes:** Fixed pre-existing test files (invalid namespace references, nullable type handling)

**Test Coverage (Bobbie):**
- 55 new tests across 4 new test files + 4 tests in existing file
- Factory tests (5): builder instantiation, error handling
- 3.0 builder tests (19): primitives, complex types, nullable, caching, serialization
- 3.1 builder tests (11): OpenAPI 3.1 specific behavior (Examples array, nullable handling)
- Integration tests (12): end-to-end document generation for OpenAPI 3.1
- Backward compatibility tests (4): default behavior matches pre-refactoring output
- **Result:** 100% test pass rate (56 total tests), zero regressions

**Verification:**
- Build: ✅ Succeeds (0 warnings, 0 errors)
- Tests: ✅ All 56 tests pass
- Backward Compatibility: ✅ Default `SpecVersion = OpenApi3_0` ensures zero breaking changes

**Files Modified:**
- Created: 5 new builders/factory files
- Deleted: 1 (original OpenApiSchemaBuilder)
- Modified: 8 (document builder, options, endpoints, csproj, partial classes)

**Handoff:** Feature complete, ready for merge. See orchestration logs for detailed implementation notes.

---

### 4. Correct byte/byte[] OpenAPI Schema Mappings

**Decision ID:** naomi-byte-schema-mapping  
**Owner:** Naomi (Lead / Architect)  
**Date:** 2026-03-09  
**Status:** Implemented

**Context:** PR reviewer flagged that `System.Byte` and `System.Byte[]` shared a single switch case in both schema builders, producing identical output: `type: string, format: byte`.

**Decision:** Split into distinct, spec-correct mappings:

| C# Type | OpenAPI Type | Format | Schema ID |
|---|---|---|---|
| `System.Byte` | `integer` | `uint8` | `System.Byte` |
| `System.Byte[]` | `string` | `byte` | `System.ByteArray` |

**Rationale:** Per OpenAPI 3.0.4 §4.3, `format: byte` under `type: string` means "base64-encoded characters." `System.Byte` is an 8-bit unsigned integer (0–255) and must map to `type: integer`. The `uint8` format is a well-known extension for 8-bit unsigned integers. The schema ID collision could break `$ref` resolution in documents containing both types.

**Implementation:** Bobbie wrote 14 unit tests (7 per builder) validating byte/byte[] mappings for both OpenAPI 3.0 and 3.1. All 81 tests pass (67 existing + 14 new).

**Verification:** Build ✅, all tests ✅, spec-compliant ✅. Committed and pushed.

**See:** `.squad/orchestration-log/2026-03-09T18-27-50Z-naomi.md` and `.squad/orchestration-log/2026-03-09T18-27-50Z-bobbie.md`.

---

### 5. CI Pipeline & NuGet Packaging

**Decision ID:** naomi-ci-nuget-packaging  
**Owner:** Naomi (Lead / Architect)  
**Date:** 2026-03-09  
**Status:** Implemented

Added GitHub Actions CI pipeline and configured the library for NuGet packaging with semantic versioning.

**Key Decisions:**
1. **MinVer** for versioning — tag-driven semver, no external tool installs, integrates as MSBuild package.
2. **Two-job CI pipeline:** `build-and-test` runs on PRs and pushes; `pack` runs only on push to main/tags, gated behind test success.
3. **global.json SDK pinning** at `10.0.103` with `latestFeature` rollForward for CI reproducibility.
4. **.NET 10 preview** SDK in CI via `dotnet-quality: 'preview'`.

**Files Changed:**
- `.github/workflows/ci.yml` — New CI pipeline
- `src/Function.OpenApi/Function.OpenApi.csproj` — NuGet metadata, MinVer dependency
- `global.json` — SDK version pin

**Verification:**
- Build: ✅ Succeeds (0 warnings, 0 errors)
- Tests: ✅ All pass (56/56)
- Pipeline: ✅ Valid and ready for production use

**Action Required:**
- **Espen:** Enable GitHub branch protection rule on `main` requiring `Build & Test` status check
- **Espen:** Tag `v0.1.0` when ready to set initial version (MinVer reads git tags)

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
