# ADR-001: Deployment Architecture Decision

## Status

Proposed

## Context

The Lucent project is intended as a standalone, offline installable data warehouse solution for fractional finance professionals. Its purpose is to extract accounting data from cloud accounting providers (initially Xero, later QBO/Sage), transform and normalise it locally, and expose it to MI tools like Power BI.

Current code structure emulates a microservice architecture, with projects like `Lucent.Api`, `Lucent.Scheduler`, `Lucent.Runner`, and `Lucent.Auth`. However, these are tightly coupled via in-memory state and lack inter-process communication infrastructure, making true distributed deployment infeasible.

Existing data model work began in SQL Express with traditional `landing`, `staging`, and `ods` schemas, but has proven overly complex and bloated for current needs.

## Decision

Lucent will be developed and deployed as a **modular monolith**, running as a single installable service with internal component separation. PostgreSQL will be adopted as the default warehouse backend, and a simplified ETL model will be used with scoped use of multiple schema layers.

### Architecture Summary

* **Application Type**: Modular monolith
* **Deployment**: Single service, locally installable (e.g. Windows service, Docker container)
* **Warehouse Engine**: PostgreSQL
* **Power BI Integration**: Via SQL views (`model` schema)
* **Infrastructure**: Previously split projects (`Lucent.Auth`, `Lucent.Client`, `Lucent.Resilience`) have been merged into a single `Lucent.Infrastructure` module to simplify dependencies and deployment.

### Warehouse Schema Design

#### `raw`

* Stores unprocessed JSON responses per API endpoint
* Fields: `id`, `endpoint`, `page_number`, `fetched_at`, `processed`, `payload_json`
* Purpose: Audit, debugging, recovery

#### `landing`

* Temporary or truncated tables for data shredding
* Mirrors `ods` schema structure (but no constraints)
* Cleared after successful load
* Fields include `source_run_id`, `loaded_at`

#### `ods`

* Fully typed, normalised tables
* Maintains relationships (e.g. `invoices`, `invoice_lines`)
* Indexed for query performance
* Includes data lineage via `source_run_id`
* Includes report shredding targets for P\&L, balance sheet, and trial balance
* Includes fields for period, `as_at_date`, `period_from`, `period_to` for report data

#### `model`

* SQL views (or materialised views) over `ods`
* Transforms business rules into analytic shape (e.g. flattening, currency conversions)
* Follows Power BI naming conventions (e.g. `"Invoice Id"`, `"Amount Paid"`, `"Start Date"`, `"End Date"`, `"Period From"`, `"Period To"`, `"As At Date"`)
* Dimensions (e.g. `contacts`, `accounts`) are split by business role (e.g. `model.customers`, `model.suppliers`)
* Fact tables are denormalised for line-level visibility
* Read-only schema for Power BI

### Key Practices

* Use `INSERT ... ON CONFLICT DO UPDATE` (Postgres-native upserts) for `landing` -> `ods` stage
* Apply simple audit table (`load_runs`) to track each batch
* Power BI connects via ODBC/Npgsql to `model` schema only

## Consequences

* Simplifies testing, debugging, deployment, and support for one-developer lifecycle
* Retains modular boundaries within a single process
* Provides clear path for future service decomposition (via interface boundaries and plugin-style providers)
* Eliminates SQL Express constraints and licence friction
* Enables leaner warehouse with more transparent data lineage
* Merging Infrastructure modules reduces solution complexity and improves maintainability
* Model views deliver a reporting-optimised schema, easing Power BI adoption and performance

## Diagram

A component diagram matching this ADR is included in `docs/architecture/ADR-001-architecture.png` (source `.drawio` also committed).

## Accepted By

Pending (CTO sign-off)

## Date

2 May 2025

## Related Issues

* \#1 Simplify deployment to monolith
* \#2 Migrate warehouse to PostgreSQL
* \#3 Design leaner ETL pipeline
* \#4 Merge Lucent.Auth, Lucent.Client, Lucent.Resilience into Lucent.Infrastructure
* \#5 Add report shredding and model views for Power BI integration
