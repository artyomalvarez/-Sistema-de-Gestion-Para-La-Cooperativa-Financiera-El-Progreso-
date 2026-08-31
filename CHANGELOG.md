# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased] - 2026-08-31 (America/Bogota)

### Added
- **TRM Integration**:
  - Added [`TrmResponseDto`](Cooperativa-El-Progreso.console/Models/TrmResponseDto.cs) in `Models` to map official Open Data Colombia TRM API responses.
  - Added [`ITrmService`](Cooperativa-El-Progreso.console/Services/ITrmService.cs) interface defining `Task<TrmResponseDto?> GetCurrentTrmAsync()`.
  - Added [`TrmService`](Cooperativa-El-Progreso.console/Services/TrmService.cs) implementation using asynchronous `HttpClient` with resilient `try-catch` handling to ensure zero app crashes if network/API failures occur.
  - Integrated `ITrmService` dependency injection and added [`GetBalanceInUsdAsync`](Cooperativa-El-Progreso.console/Services/TransactionService.cs) in `TransactionService` to compute associate balance in USD using the live TRM exchange rate safely.
- **Reporting**:
  - Added [`GetCooperativeSummary`](Cooperativa-El-Progreso.console/Services/TransactionService.cs) to `TransactionService`.
  - Added DTO models [`PeriodSummaryDto`](Cooperativa-El-Progreso.console/Models/PeriodSummaryDto.cs) and [`CashierSummaryDto`](Cooperativa-El-Progreso.console/Models/CashierSummaryDto.cs) in `Models/`.
  - Implemented 5 LINQ reporting methods in [`TransactionService.cs`](Cooperativa-El-Progreso.console/Services/TransactionService.cs):
    - `GetTopAssociates()`
    - `GetInactiveAssociates()`
    - `GetPeriodSummary(DateTime startDate, DateTime endDate)`
    - `GetLargestTransactions()`
    - `GetCashierSummary()`
