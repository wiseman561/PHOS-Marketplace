# ADR-0001: Repository Creation

## Status
Accepted

## Context
We need to establish a new repository for the PHOS Marketplace platform to avoid legacy system constraints and enable a clean, contracts-first approach to development.

## Decision
Create a new monorepo structure for PHOS Marketplace with the following rationale:
- Avoid legacy drag from existing systems
- Implement contracts-first development methodology
- Enable incremental import from Ojala only when specifically needed
- Establish clear separation of concerns across services

## Consequences
- Clean slate for implementing modern healthcare marketplace architecture
- Ability to define API contracts before implementation
- Reduced technical debt from legacy system dependencies
- Clear boundaries between different marketplace components

## Implementation
Repository structure established with empty directories for:
- Applications (apps/)
- Services (services/)
- Shared packages (packages/)
- Operations tooling (ops/)
- CI/CD workflows (.github/)
