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

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
