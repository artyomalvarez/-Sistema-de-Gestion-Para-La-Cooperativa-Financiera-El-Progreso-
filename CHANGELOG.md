# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased] - 2026-09-06 (America/Bogota)

### Added
- **Web API**:
  - Created [`Cooperativa-El-Progreso.api`](Cooperativa-El-Progreso.api) with ASP.NET Core (.NET 10) Minimal API, OpenAPI documentation, and full endpoints for associates, cash transactions, live TRM conversion, and LINQ management reports.
- **Database**:
  - Added [`database/schema.sql`](database/schema.sql) with DDL tables, foreign key constraints (`ON DELETE RESTRICT`), indexes, analytical views, and stored function for withdrawal business rules.
  - Added [`database/seed.sql`](database/seed.sql) with initial roles, users, associates, and sample transaction data.
- **UML & Architecture**:
  - Added Draw.io diagram files in [`docs/`](docs/) including [`Cooperativa-El-Progreso.drawio`](docs/Cooperativa-El-Progreso.drawio) (7-page master file), Use Case Diagram, Sequence Diagram, and Activity Diagram.

### Changed
- **Documentation**:
  - Updated [`README.md`](README.md) with complete system documentation, directory tree, database execution guides, standalone binary generation instructions, and REST API endpoint catalog.
