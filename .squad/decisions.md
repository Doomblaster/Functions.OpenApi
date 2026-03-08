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

### 2. Library Refactoring for Multi-Version OpenAPI Support

**Decision ID:** naomi-library-refactoring  
**Owner:** Naomi (Lead / Architect)  
**Date:** 2026-03-08  
**Status:** Proposed (awaiting team approval)

Refactor the Functions.OpenApi library to support multiple OpenAPI specification versions (3.0, 3.1, and future versions) while maintaining backward compatibility.

**Key Recommendations:**
- Introduce `IOpenApiSchemaBuilder` interface with version-specific implementations
- Create `OpenApiSchemaBuilderBase` for shared logic (caching, type reflection)
- Implement `OpenApi30SchemaBuilder` (current behavior) and `OpenApi31SchemaBuilder` (JSON Schema 2020-12)
- Use factory pattern: `OpenApiSchemaBuilderFactory.Create(document, specVersion)`
- Add `OpenApiDocumentOptions.SpecVersion` property (default: `OpenApi3_0`)
- Evaluate and improve cache mechanism (VERDICT: Keep with nullable key normalization fix)

**Rationale:**
- Current architecture hardcodes OpenAPI 3.0; breaking changes between 3.0 and 3.1 (nullable handling, JSON Schema alignment, Examples keyword)
- Version-specific builders enable clear separation of concerns while reusing common logic
- Factory pattern enables future versions without core builder changes
- Cache must be maintained: prevents duplicate schema generation and Components.Schemas collisions
- Cache bug fix: normalize nullable value types to underlying type before caching

**Risks & Mitigations:**
- Factory integration in InitializeDocument() is high-risk → comprehensive testing, byte-for-byte JSON comparison for 3.0
- Cache key normalization must handle both nullable and non-nullable paths → dedicated GetCacheKey() helper
- Endpoint spec version plumbing → all tests updated to support configurable spec version

**Estimated Effort:** 5–6 days (Amos 3–4 days implementation, Bobbie 2 days testing)

**Backward Compatibility:** Default `SpecVersion = OpenApi3_0` ensures zero breaking changes; new functionality is opt-in

**Next steps:** Awaiting approval from Espen (owner). Once consensus reached, execute Phase 1–5 (foundation, 3.1 implementation, integration, comprehensive testing, future enhancements).

**See:** `.squad/decisions/inbox/naomi-library-refactoring.md` for full specification and 14-step refactoring plan.

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
