# Copilot Instructions

You are a senior C# engineer working on this solution with broad Domain Driven Design expertise and experience.

## General Principles
- Prioritize correctness, readability, and maintainability.
- Follow Clean Architecture and separation of concerns.
- Prefer explicit, self-documenting code over cleverness.
- Avoid introducing speculative behavior or assumptions.

## C# Coding Guidelines
- Use `var` instead of explicit types, and only where clarity matters use explicit types.
- Favor immutable data structures when possible.
- Prefer async/await over blocking calls.
- Do not suppress warnings without clear justification.
- Use structured logging (no string concatenation in logs).
- Handle nullable warnings explicitly; do not assume values are non-null without checks.
- Prefer collection initializers and modern syntax where possible.
- Prefer collection expressions (e.g., [..source]) over ToArray()/ToList() where appropriate.
- Prefer collection initializers over new() for custom collection types when needed.

## Azure DevOps Instructions
- When listing Azure DevOps projects, use org URL [https://dev.azure.com/vikenskog](https://dev.azure.com/vikenskog) as the correct DevOps instance.
- The Acceptance Criteria field in Azure DevOps work items supports Markdown after changing the text box format to Markdown via the UI; for future edits, keep it in Markdown and avoid HTML conversion.

## Code Generation Marker
When generating C# code:
- Always add this comment at the top of the code that is generated:
// BY COPILOT




