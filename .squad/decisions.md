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

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
