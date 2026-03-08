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

