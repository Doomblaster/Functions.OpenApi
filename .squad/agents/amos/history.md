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

<!-- Append new learnings below. Each entry is something lasting about the project. -->
