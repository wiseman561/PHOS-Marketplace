![CI](https://github.com/yourorg/PHOS-Marketplace/actions/workflows/ci.yml/badge.svg)

# PHOS Marketplace (skeleton)

## Purpose
PHOS Marketplace is a healthcare marketplace platform designed to facilitate secure, compliant transactions between healthcare providers, payers, and patients. This monorepo serves as the foundation for building a comprehensive ecosystem that prioritizes data privacy, regulatory compliance, and seamless integration across healthcare stakeholders.

## Scope
- **Dev Console**: Administrative interface for platform management and monitoring
- **Catalog**: Service and product catalog management system
- **Review/Policy**: Policy enforcement and review workflows
- **Entitlements**: Access control and permission management
- **Billing**: Payment processing and financial transaction handling
- **Consent/Identity**: Patient consent management and identity verification

## Guiding Principles
- **Contracts-first**: API contracts define the system boundaries and enable independent development
- **Least privilege**: Minimal access permissions with explicit authorization at every layer
- **HIPAA-aware**: Built with healthcare compliance requirements as a foundational concern

## How we work
We iterate using 1-prompt development cycles, making small, focused changes that can be reviewed and integrated quickly. Each iteration builds upon the previous work while maintaining system integrity and compliance standards.
